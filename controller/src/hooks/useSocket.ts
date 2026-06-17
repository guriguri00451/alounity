"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { io, type Socket } from "socket.io-client";
import type { PlayerRole, SensorPayload, ServerAckPayload } from "@/lib/types";

interface UseSocketOptions {
  serverUrl?: string;
  roomId?: string;
  role: PlayerRole;
  autoConnect?: boolean;
}

interface UseSocketReturn {
  isConnected: boolean;
  playerId: string | null;
  connectionError: string | null;
  connect: () => void;
  disconnect: () => void;
  sendSensorData: (data: SensorPayload) => void;
}

export function useSocket(options: UseSocketOptions): UseSocketReturn {
  const {
    serverUrl = typeof window !== "undefined"
      ? `${window.location.protocol}//${window.location.hostname}:${window.location.port}`
      : "",
    roomId = "default",
    role,
    autoConnect = true,
  } = options;

  const socketRef = useRef<Socket | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [playerId, setPlayerId] = useState<string | null>(null);
  const [connectionError, setConnectionError] = useState<string | null>(null);
  const roleRef = useRef(role);
  const roomIdRef = useRef(roomId);

  useEffect(() => {
    roleRef.current = role;
  }, [role]);

  useEffect(() => {
    roomIdRef.current = roomId;
  }, [roomId]);

  const connect = useCallback(() => {
    if (socketRef.current?.connected) return;

    const socket = io(serverUrl, {
      transports: ["websocket", "polling"],
      reconnection: true,
      reconnectionAttempts: 10,
      reconnectionDelay: 1000,
      reconnectionDelayMax: 5000,
    });

    socket.on("connect", () => {
      setIsConnected(true);
      setConnectionError(null);

      socket.emit("controller:connect", {
        roomId: roomIdRef.current,
        role: roleRef.current,
      });
    });

    socket.on("server:ack", (data: ServerAckPayload) => {
      if (data.received && data.playerId) {
        setPlayerId(data.playerId);
      }
    });

    socket.on("disconnect", (reason) => {
      setIsConnected(false);
      if (reason === "io server disconnect") {
        setConnectionError("サーバーから切断されました");
      }
    });

    socket.on("connect_error", (error) => {
      setConnectionError(`接続エラー: ${error.message}`);
    });

    socketRef.current = socket;
  }, [serverUrl]);

  const disconnect = useCallback(() => {
    if (socketRef.current) {
      socketRef.current.removeAllListeners();
      socketRef.current.disconnect();
      socketRef.current = null;
    }
    setIsConnected(false);
    setPlayerId(null);
    setConnectionError(null);
  }, []);

  const sendSensorData = useCallback((data: SensorPayload) => {
    const socket = socketRef.current;
    if (!socket?.connected) return;

    socket.emit("controller:sensor", {
      roomId: roomIdRef.current,
      role: roleRef.current,
      accel: data.accel,
      rotation: data.rotation,
      orientation: data.orientation,
      timestamp: data.timestamp,
    });
  }, []);

  useEffect(() => {
    if (autoConnect) {
      connect();
    }
    return () => {
      disconnect();
    };
  }, [autoConnect, connect, disconnect]);

  return {
    isConnected,
    playerId,
    connectionError,
    connect,
    disconnect,
    sendSensorData,
  };
}
