"use client";

import { useCallback, useState } from "react";
import { OceanBackground } from "./OceanBackground";

interface PermissionRequestableEvent {
  requestPermission?: () => Promise<PermissionState>;
}

interface PermissionRequestProps {
  onPermissionGranted: () => void;
  onPermissionDenied?: () => void;
}

export function PermissionRequest({
  onPermissionGranted,
  onPermissionDenied,
}: PermissionRequestProps) {
  const [permissionState, setPermissionState] = useState<
    "idle" | "granted" | "denied" | "unsupported"
  >("idle");
  const [isLoading, setIsLoading] = useState(false);

  const requestPermission = useCallback(async () => {
    setIsLoading(true);

    try {
      let motionGranted = true;
      let orientationGranted = true;

      const MotionEvent = DeviceMotionEvent as unknown as PermissionRequestableEvent;
      if (
        typeof DeviceMotionEvent !== "undefined" &&
        typeof MotionEvent.requestPermission === "function"
      ) {
        const motionPermission = await MotionEvent.requestPermission();
        motionGranted = motionPermission === "granted";
      }

      const OrientationEvent = DeviceOrientationEvent as unknown as PermissionRequestableEvent;
      if (
        typeof DeviceOrientationEvent !== "undefined" &&
        typeof OrientationEvent.requestPermission === "function"
      ) {
        const orientationPermission = await OrientationEvent.requestPermission();
        orientationGranted = orientationPermission === "granted";
      }

      if (motionGranted && orientationGranted) {
        setPermissionState("granted");
        onPermissionGranted();
      } else {
        setPermissionState("denied");
        onPermissionDenied?.();
      }
    } catch {
      setPermissionState("unsupported");
      onPermissionDenied?.();
    } finally {
      setIsLoading(false);
    }
  }, [onPermissionGranted, onPermissionDenied]);

  if (permissionState === "granted") {
    return null;
  }

  return (
    <div className="relative min-h-screen flex items-center justify-center p-4">
      <OceanBackground />

      <div className="relative z-10 w-full max-w-sm">
        <div className="bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-6 text-center animate-slide-up">
          <div className="text-6xl mb-4 animate-bounce">📱</div>
          <h1 className="text-2xl font-black text-gray-800 mb-2">センサー準備完了！</h1>
          <p className="text-gray-600 text-sm mb-6">
            ゲームを始めるには
            <br />
            センサーアクセスを許可してください
          </p>

          {permissionState === "unsupported" && (
            <div className="mb-4 p-3 bg-red-100 rounded-xl">
              <p className="font-bold text-red-700 text-sm">❌ 非対応ブラウザ</p>
              <p className="text-xs text-red-600 mt-1">
                iOS Safari（13+）またはChrome（Android）を使用してください。
              </p>
            </div>
          )}

          {permissionState === "denied" && (
            <div className="mb-4 p-3 bg-yellow-100 rounded-xl">
              <p className="font-bold text-yellow-700 text-sm">⚠️ 権限が拒否されました</p>
              <p className="text-xs text-yellow-600 mt-1">
                ブラウザの設定で権限を許可してください。
              </p>
            </div>
          )}

          <div className="mb-6 bg-gray-50 rounded-xl p-4 text-left">
            <p className="text-xs font-bold text-gray-700 mb-2">🎮 使用するセンサー：</p>
            <ul className="text-xs text-gray-600 space-y-1">
              <li>
                📳 <strong>加速度センサー</strong> — スマホを振る動作
              </li>
              <li>
                🧭 <strong>ジャイロセンサー</strong> — 回転・向きを検出
              </li>
            </ul>
          </div>

          <button
            type="button"
            onClick={requestPermission}
            disabled={isLoading}
            className="w-full py-4 rounded-full font-black text-lg bg-gradient-to-r from-blue-500 to-blue-600 text-white shadow-lg hover:scale-[1.02] active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? "⏳ 許可中..." : "🚀 スタート！"}
          </button>

          <p className="mt-3 text-xs text-gray-400">ボタンを押すと権限ダイアログが表示されます</p>
        </div>
      </div>
    </div>
  );
}
