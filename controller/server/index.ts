import { existsSync, readFileSync } from "node:fs";
import { createServer as createHttpServer } from "node:http";
import { createServer as createHttpsServer } from "node:https";
import { resolve } from "node:path";
import { parse } from "node:url";
import next from "next";
import { Server } from "socket.io";

const dev = process.env.NODE_ENV !== "production";
const hostname = "0.0.0.0";
const port = Number.parseInt(process.env.PORT || "3000", 10);

// ルーム管理
const VALID_ROLES = ["paddle_right", "paddle_left", "fisher"] as const;
type ValidRole = (typeof VALID_ROLES)[number];

const TEAMS = ["A", "B"] as const;
type Team = (typeof TEAMS)[number];

const GAME_MODES = ["single", "versus"] as const;
type GameMode = (typeof GAME_MODES)[number];

interface PlayerData {
  role: ValidRole;
  team: Team;
}

interface RoomState {
  roomId: string;
  hostId: string;
  createdAt: number;
  gameMode: GameMode;
  players: Map<string, PlayerData>; // socket.id → { role, team }
}

const rooms = new Map<string, RoomState>();

function generateRoomId(): string {
  const chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
  let id = "";
  for (let i = 0; i < 6; i++) {
    id += chars[Math.floor(Math.random() * chars.length)];
  }
  return id;
}

function emitPlayersUpdate(io: Server, roomId: string) {
  const room = rooms.get(roomId);
  if (!room) return;
  const players = Array.from(room.players.entries()).map(([playerId, data]) => ({
    playerId,
    role: data.role,
    team: data.team,
  }));
  io.to(`room:${roomId}`).emit("room:players_update", { players });
}

// HTTPS設定
const httpsEnabled = process.env.HTTPS === "true";
const certDir = resolve(process.cwd(), "certs");
const keyPath = resolve(certDir, "server-key.pem");
const certPath = resolve(certDir, "server.pem");

async function startServer() {
  const app = next({ dev, hostname, port });
  const handle = app.getRequestHandler();

  await app.prepare();

  let server: ReturnType<typeof createHttpServer>;

  if (httpsEnabled) {
    // HTTPSサーバー
    if (!existsSync(keyPath) || !existsSync(certPath)) {
      console.error("Error: HTTPS certificates not found.");
      console.error("Please run: npm run setup:https");
      process.exit(1);
    }

    const httpsOptions = {
      key: readFileSync(keyPath),
      cert: readFileSync(certPath),
    };

    server = createHttpsServer(httpsOptions, (req, res) => {
      const parsedUrl = parse(req.url!, true);
      handle(req, res, parsedUrl);
    });

    console.log(`> HTTPS enabled`);
  } else {
    // HTTPサーバー
    server = createHttpServer((req, res) => {
      const parsedUrl = parse(req.url!, true);
      handle(req, res, parsedUrl);
    });

    console.log(`> HTTP mode (set HTTPS=true for HTTPS)`);
  }

  // Socket.IOサーバーの初期化
  const io = new Server(server, {
    cors: {
      origin: "*",
      methods: ["GET", "POST"],
    },
    transports: ["websocket", "polling"],
  });

  // Socket.IO接続イベント
  io.on("connection", (socket) => {
    console.log(`[Socket.IO] Client connected: ${socket.id}`);

    // --- ホスト（Unity）用イベント ---

    // ルーム作成
    socket.on("host:create", (data) => {
      // 既にホストとしてルームを持っている場合は拒否
      for (const [, room] of rooms) {
        if (room.hostId === socket.id) {
          socket.emit("host:create_ack", { ok: false, error: "既にルームを所有しています" });
          return;
        }
      }

      const gameMode: GameMode =
        data && typeof data === "object" && data.gameMode === "versus" ? "versus" : "single";

      let roomId: string;
      do {
        roomId = generateRoomId();
      } while (rooms.has(roomId));

      rooms.set(roomId, {
        roomId,
        hostId: socket.id,
        createdAt: Date.now(),
        gameMode,
        players: new Map(),
      });

      socket.join(`room:${roomId}`);
      console.log(`[Room] 作成: ${roomId} (host: ${socket.id}, mode: ${gameMode})`);
      socket.emit("host:create_ack", { ok: true, roomId, gameMode });
    });

    // ルーム閉鎖
    socket.on("host:close", (data) => {
      if (!data || typeof data !== "object") return;
      const roomId = typeof data.roomId === "string" ? data.roomId : "";
      const room = rooms.get(roomId);

      if (!room || room.hostId !== socket.id) return;

      rooms.delete(roomId);
      io.to(`room:${roomId}`).emit("room:closed", { roomId, reason: "host_closed" });
      console.log(`[Room] 閉鎖: ${roomId}`);
    });

    // --- コントローラー（スマホ）用イベント ---

    // ルーム存在確認（事前検証用 + リアルタイム更新用）
    socket.on("room:exists", (data) => {
      if (!data || typeof data !== "object") {
        socket.emit("room:exists_ack", { exists: false });
        return;
      }
      const roomId = typeof data.roomId === "string" ? data.roomId : "";
      const room = rooms.get(roomId);
      if (!room) {
        socket.emit("room:exists_ack", { exists: false });
        return;
      }

      // ルームに参加してリアルタイム更新を受け取る
      socket.join(`room:${roomId}`);

      // チーム別の占有状態を構築
      const takenRoles: Record<string, string[]> = {};
      for (const team of TEAMS) {
        takenRoles[team] = Array.from(room.players.values())
          .filter((p) => p.team === team)
          .map((p) => p.role);
      }

      socket.emit("room:exists_ack", {
        exists: true,
        gameMode: room.gameMode,
        takenRoles,
      });
    });

    // コントローラー接続
    socket.on("controller:connect", (data) => {
      if (!data || typeof data !== "object") {
        socket.emit("server:ack", { received: false, error: "Invalid data" });
        return;
      }

      const roomId = typeof data.roomId === "string" ? data.roomId : "";
      const role = typeof data.role === "string" ? data.role : "";
      const team = typeof data.team === "string" ? data.team : "A";

      // ルーム存在確認
      if (!roomId || !rooms.has(roomId)) {
        socket.emit("server:ack", {
          received: false,
          error: "ルームが見つかりません",
        });
        return;
      }

      // 役割の妥当性チェック
      if (!VALID_ROLES.includes(role as ValidRole)) {
        socket.emit("server:ack", {
          received: false,
          error: "無効な役割です",
        });
        return;
      }

      const room = rooms.get(roomId)!;

      // チームの妥当性チェック
      if (!TEAMS.includes(team as Team)) {
        socket.emit("server:ack", {
          received: false,
          error: "無効なチームです",
        });
        return;
      }

      // singleモードではチームAのみ許可
      if (room.gameMode === "single" && team !== "A") {
        socket.emit("server:ack", {
          received: false,
          error: "このモードではチームAのみ選択できます",
        });
        return;
      }

      // チーム内での役割の重複チェック
      for (const [playerId, playerData] of room.players) {
        if (
          playerData.role === role &&
          playerData.team === (team as Team) &&
          playerId !== socket.id
        ) {
          socket.emit("server:ack", {
            received: false,
            error: "この役割は既に使用されています",
          });
          return;
        }
      }

      // プレイヤーを登録
      room.players.set(socket.id, { role: role as ValidRole, team: team as Team });
      socket.data.roomId = roomId;
      socket.data.role = role;
      socket.data.team = team;

      console.log(
        `[Controller] 参加: room:${roomId}, team:${team}, role:${role}, player:${socket.id}`
      );
      socket.join(`room:${roomId}`);
      socket.emit("server:ack", { received: true, playerId: socket.id });

      // ルーム内の全クライアントに役割更新を通知
      emitPlayersUpdate(io, roomId);
    });

    // センサーデータ受信（30fpsスロットリング）
    socket.on("controller:sensor", (data) => {
      if (!data || typeof data !== "object") return;
      const roomId = typeof data.roomId === "string" ? data.roomId : "";
      if (!roomId || !rooms.has(roomId)) return;

      const now = Date.now();
      const lastSent = socket.data.lastSensorSent || 0;
      if (now - lastSent < 33) return;
      socket.data.lastSensorSent = now;

      const payload = {
        playerId: socket.id,
        role: typeof data.role === "string" ? data.role : "unknown",
        team: (socket.data.team as string) || "A",
        accel: data.accel && typeof data.accel === "object" ? data.accel : null,
        rotation: data.rotation && typeof data.rotation === "object" ? data.rotation : null,
        orientation:
          data.orientation && typeof data.orientation === "object" ? data.orientation : null,
        timestamp: typeof data.timestamp === "number" ? data.timestamp : Date.now(),
      };
      io.to(`room:${roomId}`).emit("sensor:data", payload);
    });

    // --- 共通イベント ---

    // 切断
    socket.on("disconnect", (reason) => {
      console.log(`[Socket.IO] Client disconnected: ${socket.id}, reason: ${reason}`);

      // コントローラー切断 → ルームからプレイヤー削除
      const controllerRoomId = socket.data.roomId as string | undefined;
      if (controllerRoomId) {
        const room = rooms.get(controllerRoomId);
        if (room) {
          const playerData = room.players.get(socket.id);
          room.players.delete(socket.id);
          if (playerData) {
            console.log(
              `[Controller] 退出: room:${controllerRoomId}, team:${playerData.team}, role:${playerData.role}, player:${socket.id}`
            );
            // ルーム内の全クライアントに役割更新を通知
            emitPlayersUpdate(io, controllerRoomId);
          }
        }
      }

      // ホスト切断 → ルーム削除
      for (const [roomId, room] of rooms) {
        if (room.hostId === socket.id) {
          rooms.delete(roomId);
          io.to(`room:${roomId}`).emit("room:closed", { roomId, reason: "host_disconnected" });
          console.log(`[Room] 削除（ホスト切断）: ${roomId}`);
          break;
        }
      }
    });
  });

  const protocol = httpsEnabled ? "https" : "http";
  const localIp = process.env.LOCAL_IP;
  server.listen(port, hostname, () => {
    console.log(`> Ready on ${protocol}://${hostname}:${port}`);
    console.log(`> Socket.IO server is running`);
    if (localIp) {
      console.log(`> Access from mobile: ${protocol}://${localIp}:${port}`);
    }
  });
}

startServer().catch((error) => {
  console.error("Failed to start server:", error);
  process.exit(1);
});
