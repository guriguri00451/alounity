import { createServer } from "node:http";
import { parse } from "node:url";
import next from "next";
import { Server } from "socket.io";

const dev = process.env.NODE_ENV !== "production";
const hostname = "localhost";
const port = 3000;

async function startServer() {
  const app = next({ dev, hostname, port });
  const handle = app.getRequestHandler();

  await app.prepare();

  const server = createServer((req, res) => {
    const parsedUrl = parse(req.url!, true);
    handle(req, res, parsedUrl);
  });

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
      console.log(`[Socket.IO] Controller connected:`, data);
      socket.join(`room:${data.roomId || "default"}`);
      socket.emit("server:ack", { received: true, playerId: socket.id });
    });

    // センサーデータ受信イベント
    socket.on("controller:sensor", (data) => {
      // 同じルーム内のUnityクライアントに転送
      io.to(`room:${data.roomId || "default"}`).emit("sensor:data", {
        playerId: socket.id,
        ...data,
      });
    });

    // Unity接続イベント
    socket.on("unity:connect", (data) => {
      console.log(`[Socket.IO] Unity connected:`, data);
      socket.join(`room:${data.roomId || "default"}`);
    });

    // 切断イベント
    socket.on("disconnect", (reason) => {
      console.log(`[Socket.IO] Client disconnected: ${socket.id}, reason: ${reason}`);
    });
  });

  server.listen(port, () => {
    console.log(`> Ready on http://${hostname}:${port}`);
    console.log(`> Socket.IO server is running`);
  });
}

startServer().catch((error) => {
  console.error("Failed to start server:", error);
  process.exit(1);
});
