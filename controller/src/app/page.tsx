"use client";

import { useCallback, useState } from "react";
import { PermissionRequest } from "@/components/PermissionRequest";
import { SensorDisplay } from "@/components/SensorDisplay";
import { useDeviceMotion } from "@/hooks/useDeviceMotion";

export default function Home() {
  const [hasPermission, setHasPermission] = useState(false);
  const [isListening, setIsListening] = useState(false);
  const { sensorData, isSupported, startListening, stopListening } = useDeviceMotion({
    throttleMs: 33, // 30fps
  });

  const handlePermissionGranted = useCallback(() => {
    setHasPermission(true);
    setIsListening(true);
    startListening();
  }, [startListening]);

  const handlePermissionDenied = useCallback(() => {
    console.log("Permission denied");
  }, []);

  // センサーAPIがサポートされていない場合
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

  // 権限が許可されていない場合
  if (!hasPermission) {
    return (
      <PermissionRequest
        onPermissionGranted={handlePermissionGranted}
        onPermissionDenied={handlePermissionDenied}
      />
    );
  }

  // メイン画面
  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-2xl mx-auto space-y-4">
        <header className="text-center py-4">
          <h1 className="text-2xl font-bold text-gray-800">スマホコントローラー</h1>
          <p className="text-sm text-gray-600 mt-1">センサーデータをリアルタイムで表示</p>
        </header>

        <SensorDisplay sensorData={sensorData} />

        <div className="bg-white rounded-lg shadow-md p-4">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">センサー状態</h3>
          <div className="text-xs text-gray-600 space-y-1">
            <p>
              状態: <span className="text-green-600 font-semibold">リッスン中</span>
            </p>
            <p>スロットル: 30fps (33ms)</p>
          </div>
        </div>

        <div className="text-center pt-4">
          <button
            type="button"
            onClick={isListening ? stopListening : startListening}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-700 rounded-lg text-sm"
          >
            {isListening ? "センサー停止" : "センサー再開"}
          </button>
        </div>
      </div>
    </div>
  );
}
