import { existsSync, readFileSync } from "node:fs";
import { createServer as createHttpServer } from "node:http";
import { createServer as createHttpsServer } from "node:https";
import { resolve } from "node:path";
import { parse } from "node:url";
import next from "next";
import { Server } from "socket.io";

const dev = process.env.NODE_ENV !== "production";
const hostname = "0.0.0.0"; // すべてのネットワークインターフェースでリッスン
const port = Number.parseInt(process.env.PORT || "3000", 10);

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

    // コントローラー接続イベント
    socket.on("controller:connect", (data) => {
      if (!data || typeof data !== "object") {
        socket.emit("server:ack", { received: false, error: "Invalid data" });
        return;
      }
      const roomId = typeof data.roomId === "string" ? data.roomId : "default";
      console.log(`[Socket.IO] Controller connected:`, data);
      socket.join(`room:${roomId}`);
      socket.emit("server:ack", { received: true, playerId: socket.id });
    });

    // センサーデータ受信イベント
    socket.on("controller:sensor", (data) => {
      if (!data || typeof data !== "object") return;
      const roomId = typeof data.roomId === "string" ? data.roomId : "default";
      // バリデーション済みのフィールドのみ転送
      const payload = {
        playerId: socket.id,
        role: typeof data.role === "string" ? data.role : "unknown",
        accel: data.accel && typeof data.accel === "object" ? data.accel : null,
        rotation: data.rotation && typeof data.rotation === "object" ? data.rotation : null,
        orientation:
          data.orientation && typeof data.orientation === "object" ? data.orientation : null,
        timestamp: typeof data.timestamp === "number" ? data.timestamp : Date.now(),
      };
      io.to(`room:${roomId}`).emit("sensor:data", payload);
    });

    // Unity接続イベント
    socket.on("unity:connect", (data) => {
      if (!data || typeof data !== "object") return;
      const roomId = typeof data.roomId === "string" ? data.roomId : "default";
      console.log(`[Socket.IO] Unity connected:`, data);
      socket.join(`room:${roomId}`);
    });

    // 切断イベント
    socket.on("disconnect", (reason) => {
      console.log(`[Socket.IO] Client disconnected: ${socket.id}, reason: ${reason}`);
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
