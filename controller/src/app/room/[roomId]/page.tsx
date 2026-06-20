"use client";

import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { io } from "socket.io-client";
import { PermissionRequest } from "@/components/PermissionRequest";
import { RoleSelector } from "@/components/RoleSelector";
import { SensorDebugOverlay } from "@/components/SensorDebugOverlay";
import { SensorDisplay } from "@/components/SensorDisplay";
import { useDeviceMotion } from "@/hooks/useDeviceMotion";
import { useSocket } from "@/hooks/useSocket";
import type { PlayerRole } from "@/lib/types";

const ALL_ROLES: PlayerRole[] = ["paddle_right", "paddle_left", "fisher"];

export default function RoomPage() {
  const params = useParams();
  const roomId = typeof params.roomId === "string" ? params.roomId : "";

  const [role, setRole] = useState<PlayerRole | null>(null);
  const [hasPermission, setHasPermission] = useState(false);
  const [isListening, setIsListening] = useState(false);
  const [takenRoles, setTakenRoles] = useState<PlayerRole[]>([]);
  const [rolesLoading, setRolesLoading] = useState(true);

  const { sensorData, isSupported, startListening, stopListening } = useDeviceMotion({
    throttleMs: 33,
  });

  const { isConnected, playerId, connectionError, sendSensorData } = useSocket({
    roomId,
    role: role ?? "paddle_right",
    autoConnect: role !== null,
  });

  // 役割の占有状態をリアルタイム取得（役割選択前のみ接続）
  useEffect(() => {
    if (!roomId || role) return;

    const serverUrl = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
    const socket = io(serverUrl, {
      transports: ["websocket", "polling"],
      reconnection: false,
    });

    const timeout = setTimeout(() => {
      socket.disconnect();
      setRolesLoading(false);
    }, 5000);

    socket.on("connect", () => {
      socket.emit("room:exists", { roomId });
    });

    socket.on("room:exists_ack", (data) => {
      clearTimeout(timeout);
      setRolesLoading(false);

      if (data.exists && data.takenRoles) {
        setTakenRoles(data.takenRoles as PlayerRole[]);
      }
    });

    // リアルタイム更新：他のプレイヤーが参加/退出した際に役割状態を更新
    socket.on("room:players_update", (data) => {
      if (data.takenRoles) {
        setTakenRoles(data.takenRoles as PlayerRole[]);
      }
    });

    socket.on("connect_error", () => {
      clearTimeout(timeout);
      socket.disconnect();
      setRolesLoading(false);
    });

    return () => {
      clearTimeout(timeout);
      socket.disconnect();
    };
  }, [roomId, role]);

  // 接続エラー時に役割選択画面に戻る
  useEffect(() => {
    if (connectionError === "この役割は既に使用されています") {
      setRole(null);
    }
  }, [connectionError]);

  useEffect(() => {
    if (!sensorData || !isListening || !isConnected) return;

    sendSensorData({
      accel: sensorData.acceleration
        ? {
            x: sensorData.acceleration.x ?? 0,
            y: sensorData.acceleration.y ?? 0,
            z: sensorData.acceleration.z ?? 0,
          }
        : null,
      rotation: sensorData.rotationRate
        ? {
            alpha: sensorData.rotationRate.alpha ?? 0,
            beta: sensorData.rotationRate.beta ?? 0,
            gamma: sensorData.rotationRate.gamma ?? 0,
          }
        : null,
      orientation: sensorData.orientation
        ? {
            alpha: sensorData.orientation.alpha ?? 0,
            beta: sensorData.orientation.beta ?? 0,
            gamma: sensorData.orientation.gamma ?? 0,
          }
        : null,
      timestamp: sensorData.timestamp,
    });
  }, [sensorData, isListening, isConnected, sendSensorData]);

  const handlePermissionGranted = useCallback(() => {
    setHasPermission(true);
    setIsListening(true);
    startListening();
  }, [startListening]);

  const handlePermissionDenied = useCallback(() => {
    console.log("Permission denied");
  }, []);

  const handleToggleListening = useCallback(() => {
    if (isListening) {
      setIsListening(false);
      stopListening();
    } else {
      setIsListening(true);
      startListening();
    }
  }, [isListening, startListening, stopListening]);

  if (!isSupported) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gray-50">
        <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-6 text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-4">未対応ブラウザ</h1>
          <p className="text-gray-700">
            このブラウザはDeviceMotion/DeviceOrientation APIをサポートしていません。
          </p>
          <p className="text-sm text-gray-500 mt-2">
            iOS Safari（13+）またはChrome（Android）を使用してください。
          </p>
        </div>
      </div>
    );
  }

  if (!role) {
    if (rolesLoading) {
      return (
        <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">ルーム情報を取得中...</p>
          </div>
        </div>
      );
    }

    const allTaken = takenRoles.length >= ALL_ROLES.length;
    if (allTaken) {
      return (
        <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
          <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-6 text-center">
            <h1 className="text-2xl font-bold text-red-600 mb-4">満席です</h1>
            <p className="text-gray-700">
              このルームは全ての役割が使用中です。別のルームに参加するか、しばらく待ってから再度お試しください。
            </p>
          </div>
        </div>
      );
    }

    return <RoleSelector onSelect={setRole} disabledRoles={takenRoles} />;
  }

  if (!hasPermission) {
    return (
      <PermissionRequest
        onPermissionGranted={handlePermissionGranted}
        onPermissionDenied={handlePermissionDenied}
      />
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <SensorDebugOverlay sensorData={sensorData} isConnected={isConnected} role={role} />
      <div className="max-w-2xl mx-auto space-y-4">
        <header className="text-center py-4">
          <h1 className="text-2xl font-bold text-gray-800">スマホコントローラー</h1>
          <p className="text-sm text-gray-600 mt-1">
            ルーム: <span className="font-mono font-bold tracking-wider">{roomId}</span>
          </p>
        </header>

        <SensorDisplay sensorData={sensorData} />

        <div className="bg-white rounded-lg shadow-md p-4 space-y-2">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">センサー状態</h3>
          <div className="text-xs text-gray-600 space-y-1">
            <p>
              状態:{" "}
              <span className={`font-semibold ${isListening ? "text-green-600" : "text-gray-500"}`}>
                {isListening ? "リッスン中" : "停止中"}
              </span>
            </p>
            <p>スロットル: 30fps (33ms)</p>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-md p-4 space-y-2">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">接続状態</h3>
          <div className="text-xs text-gray-600 space-y-1">
            <p>
              ルーム: <span className="font-mono font-semibold text-gray-800">{roomId}</span>
            </p>
            <p>
              Socket.IO:{" "}
              <span className={`font-semibold ${isConnected ? "text-green-600" : "text-red-500"}`}>
                {isConnected ? "接続済み" : "未接続"}
              </span>
            </p>
            {playerId && <p>プレイヤーID: {playerId.slice(0, 8)}...</p>}
            {connectionError && <p className="text-red-500">{connectionError}</p>}
          </div>
        </div>

        <div className="text-center pt-4">
          <button
            type="button"
            onClick={handleToggleListening}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-700 rounded-lg text-sm"
          >
            {isListening ? "センサー停止" : "センサー再開"}
          </button>
        </div>
      </div>
    </div>
  );
}
