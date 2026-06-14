"use client";

import { useCallback, useEffect, useRef, useState } from "react";

export interface SensorData {
  // DeviceMotionEvent
  acceleration: { x: number | null; y: number | null; z: number | null } | null;
  accelerationIncludingGravity: { x: number | null; y: number | null; z: number | null } | null;
  rotationRate: { alpha: number | null; beta: number | null; gamma: number | null } | null;

  // DeviceOrientationEvent
  orientation: { alpha: number | null; beta: number | null; gamma: number | null } | null;

  // タイムスタンプ
  timestamp: number;
}

interface UseDeviceMotionOptions {
  throttleMs?: number; // スロットル間隔（ミリ秒）
}

export function useDeviceMotion(options: UseDeviceMotionOptions = {}) {
  const { throttleMs = 33 } = options; // デフォルト30fps（33ms間隔）

  const [sensorData, setSensorData] = useState<SensorData | null>(null);
  const [isSupported, setIsSupported] = useState(true);
  const lastSentRef = useRef<number>(0);

  // センサーAPIのサポート確認
  useEffect(() => {
    if (typeof window === "undefined") return;

    const hasDeviceMotion = "DeviceMotionEvent" in window;
    const hasDeviceOrientation = "DeviceOrientationEvent" in window;

    if (!hasDeviceMotion && !hasDeviceOrientation) {
      setIsSupported(false);
    }
  }, []);

  // DeviceMotionEventハンドラー
  const handleDeviceMotion = useCallback(
    (event: DeviceMotionEvent) => {
      const now = Date.now();

      // スロットリング
      if (now - lastSentRef.current < throttleMs) {
        return;
      }
      lastSentRef.current = now;

      const data: SensorData = {
        acceleration: event.acceleration
          ? {
              x: event.acceleration.x,
              y: event.acceleration.y,
              z: event.acceleration.z,
            }
          : null,
        accelerationIncludingGravity: event.accelerationIncludingGravity
          ? {
              x: event.accelerationIncludingGravity.x,
              y: event.accelerationIncludingGravity.y,
              z: event.accelerationIncludingGravity.z,
            }
          : null,
        rotationRate: event.rotationRate
          ? {
              alpha: event.rotationRate.alpha,
              beta: event.rotationRate.beta,
              gamma: event.rotationRate.gamma,
            }
          : null,
        orientation: null, // DeviceOrientationEventで取得
        timestamp: now,
      };

      setSensorData(data);
    },
    [throttleMs]
  );

  // DeviceOrientationEventハンドラー
  const handleDeviceOrientation = useCallback((event: DeviceOrientationEvent) => {
    setSensorData((prev) => ({
      ...prev!,
      orientation: {
        alpha: event.alpha,
        beta: event.beta,
        gamma: event.gamma,
      },
      timestamp: Date.now(),
    }));
  }, []);

  // イベントリスナーの登録
  const startListening = useCallback(() => {
    if (typeof window === "undefined") return;

    window.addEventListener("devicemotion", handleDeviceMotion);
    window.addEventListener("deviceorientation", handleDeviceOrientation);
  }, [handleDeviceMotion, handleDeviceOrientation]);

  // イベントリスナーの解除
  const stopListening = useCallback(() => {
    if (typeof window === "undefined") return;

    window.removeEventListener("devicemotion", handleDeviceMotion);
    window.removeEventListener("deviceorientation", handleDeviceOrientation);
  }, [handleDeviceMotion, handleDeviceOrientation]);

  // クリーンアップ
  useEffect(() => {
    return () => {
      stopListening();
    };
  }, [stopListening]);

  return {
    sensorData,
    isSupported,
    startListening,
    stopListening,
  };
}
