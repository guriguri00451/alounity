"use client";

import { useCallback, useState } from "react";

// DeviceMotionEvent/DeviceOrientationEventのrequestPermissionメソッド型定義
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
      // DeviceMotionEventの権限リクエスト（iOS 13+）
      const MotionEvent = DeviceMotionEvent as unknown as PermissionRequestableEvent;
      if (
        typeof DeviceMotionEvent !== "undefined" &&
        typeof MotionEvent.requestPermission === "function"
      ) {
        const motionPermission = await MotionEvent.requestPermission();

        if (motionPermission !== "granted") {
          setPermissionState("denied");
          onPermissionDenied?.();
          setIsLoading(false);
          return;
        }
      }

      // DeviceOrientationEventの権限リクエスト（iOS 13+）
      const OrientationEvent = DeviceOrientationEvent as unknown as PermissionRequestableEvent;
      if (
        typeof DeviceOrientationEvent !== "undefined" &&
        typeof OrientationEvent.requestPermission === "function"
      ) {
        const orientationPermission = await OrientationEvent.requestPermission();

        if (orientationPermission !== "granted") {
          setPermissionState("denied");
          onPermissionDenied?.();
          setIsLoading(false);
          return;
        }
      }

      // 両方の権限が許可された
      setPermissionState("granted");
      onPermissionGranted();
    } catch (error) {
      console.error("Permission request failed:", error);
      setPermissionState("unsupported");
      onPermissionDenied?.();
    } finally {
      setIsLoading(false);
    }
  }, [onPermissionGranted, onPermissionDenied]);

  // 権限が既に許可されている場合
  if (permissionState === "granted") {
    return null;
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gray-50">
      <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-6">
        <h1 className="text-2xl font-bold text-center mb-4">センサーアクセス許可</h1>

        {permissionState === "unsupported" && (
          <div className="mb-4 p-3 bg-red-100 text-red-700 rounded">
            <p className="font-semibold">サポートされていないブラウザ</p>
            <p className="text-sm">
              このブラウザはDeviceMotion/DeviceOrientation APIをサポートしていません。 iOS
              Safari（13+）またはChrome（Android）を使用してください。
            </p>
          </div>
        )}

        {permissionState === "denied" && (
          <div className="mb-4 p-3 bg-yellow-100 text-yellow-700 rounded">
            <p className="font-semibold">権限が拒否されました</p>
            <p className="text-sm">
              センサーデータを使用するには、ブラウザの設定で権限を許可してください。
            </p>
          </div>
        )}

        <div className="mb-6 text-gray-700">
          <p className="mb-2">このアプリは以下のセンサーを使用します：</p>
          <ul className="list-disc list-inside space-y-1 text-sm">
            <li>
              <strong>加速度センサー</strong> - スマホを振る動作を検出
            </li>
            <li>
              <strong>ジャイロセンサー</strong> - スマホの回転・向きを検出
            </li>
          </ul>
          <p className="mt-3 text-sm text-gray-600">※ iOSの場合、HTTPS接続が必要です</p>
        </div>

        <button
          type="button"
          onClick={requestPermission}
          disabled={isLoading}
          className="w-full py-3 px-4 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white font-semibold rounded-lg transition-colors"
        >
          {isLoading ? "許可をリクエスト中..." : "センサーアクセスを許可"}
        </button>

        <p className="mt-4 text-xs text-gray-500 text-center">
          ボタンを押すとブラウザの権限ダイアログが表示されます
        </p>
      </div>
    </div>
  );
}
