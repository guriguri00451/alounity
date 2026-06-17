"use client";

import { useEffect, useState } from "react";
import type { SensorData } from "@/hooks/useDeviceMotion";
import type { PlayerRole } from "@/lib/types";

interface SensorDebugOverlayProps {
  sensorData: SensorData | null;
  isConnected: boolean;
  role: PlayerRole | null;
}

export function SensorDebugOverlay({ sensorData, isConnected, role }: SensorDebugOverlayProps) {
  const [isVisible, setIsVisible] = useState(false);
  const [lastSentTime, setLastSentTime] = useState<number | null>(null);

  useEffect(() => {
    if (sensorData) {
      setLastSentTime(sensorData.timestamp);
    }
  }, [sensorData]);

  const timeSinceLastSent = lastSentTime ? Math.floor((Date.now() - lastSentTime) / 1000) : null;

  if (!isVisible) {
    return (
      <button
        type="button"
        onClick={() => setIsVisible(true)}
        className="fixed top-2 right-2 z-50 px-2 py-1 bg-black/70 text-white text-xs rounded-full"
      >
        DEBUG
      </button>
    );
  }

  return (
    <div className="fixed top-2 right-2 z-50 w-64 bg-black/85 text-white text-xs rounded-lg p-3 font-mono shadow-lg">
      <div className="flex justify-between items-center mb-2 border-b border-white/20 pb-1">
        <span className="font-bold">DEBUG OVERLAY</span>
        <button
          type="button"
          onClick={() => setIsVisible(false)}
          className="text-white/60 hover:text-white"
        >
          ✕
        </button>
      </div>

      <div className="space-y-1.5">
        <div className="flex justify-between">
          <span className="text-white/60">Role:</span>
          <span>{role ?? "N/A"}</span>
        </div>

        <div className="flex justify-between">
          <span className="text-white/60">Socket:</span>
          <span className={isConnected ? "text-green-400" : "text-red-400"}>
            {isConnected ? "● connected" : "○ disconnected"}
          </span>
        </div>

        <div className="flex justify-between">
          <span className="text-white/60">Last sent:</span>
          <span
            className={timeSinceLastSent !== null && timeSinceLastSent > 2 ? "text-red-400" : ""}
          >
            {timeSinceLastSent !== null ? `${timeSinceLastSent}秒前` : "N/A"}
          </span>
        </div>

        {sensorData?.acceleration && (
          <div className="pt-1 border-t border-white/20">
            <div className="text-white/60 mb-0.5">Accel [m/s²]</div>
            <div className="grid grid-cols-3 gap-1">
              <span>X: {sensorData.acceleration.x?.toFixed(2) ?? "-"}</span>
              <span>Y: {sensorData.acceleration.y?.toFixed(2) ?? "-"}</span>
              <span>Z: {sensorData.acceleration.z?.toFixed(2) ?? "-"}</span>
            </div>
          </div>
        )}

        {sensorData?.rotationRate && (
          <div>
            <div className="text-white/60 mb-0.5">Rotation [deg/s]</div>
            <div className="grid grid-cols-3 gap-1">
              <span>α: {sensorData.rotationRate.alpha?.toFixed(1) ?? "-"}</span>
              <span>β: {sensorData.rotationRate.beta?.toFixed(1) ?? "-"}</span>
              <span>γ: {sensorData.rotationRate.gamma?.toFixed(1) ?? "-"}</span>
            </div>
          </div>
        )}

        {sensorData?.orientation && (
          <div>
            <div className="text-white/60 mb-0.5">Orientation [deg]</div>
            <div className="grid grid-cols-3 gap-1">
              <span>α: {sensorData.orientation.alpha?.toFixed(1) ?? "-"}</span>
              <span>β: {sensorData.orientation.beta?.toFixed(1) ?? "-"}</span>
              <span>γ: {sensorData.orientation.gamma?.toFixed(1) ?? "-"}</span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
