"use client";

import { useCallback, useEffect, useState } from "react";
import { PermissionRequest } from "@/components/PermissionRequest";
import { RoleSelector } from "@/components/RoleSelector";
import { SensorDebugOverlay } from "@/components/SensorDebugOverlay";
import { SensorDisplay } from "@/components/SensorDisplay";
import { useDeviceMotion } from "@/hooks/useDeviceMotion";
import { useSocket } from "@/hooks/useSocket";
import type { PlayerRole } from "@/lib/types";

export default function Home() {
  const [role, setRole] = useState<PlayerRole | null>(null);
  const [hasPermission, setHasPermission] = useState(false);
  const [isListening, setIsListening] = useState(false);

  const { sensorData, isSupported, startListening, stopListening } = useDeviceMotion({
    throttleMs: 33,
  });

  const { isConnected, playerId, connectionError, sendSensorData } = useSocket({
    role: role ?? "paddle_right",
    autoConnect: role !== null,
  });

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
    return <RoleSelector onSelect={setRole} />;
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
          <p className="text-sm text-gray-600 mt-1">センサーデータをリアルタイムで送信</p>
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
