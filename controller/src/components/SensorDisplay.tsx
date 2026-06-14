"use client";

import type { SensorData } from "@/hooks/useDeviceMotion";

interface SensorDisplayProps {
  sensorData: SensorData | null;
}

export function SensorDisplay({ sensorData }: SensorDisplayProps) {
  if (!sensorData) {
    return (
      <div className="p-4 bg-gray-100 rounded-lg text-center text-gray-500">
        センサーデータを待機中...
      </div>
    );
  }

  return (
    <div className="p-4 bg-white rounded-lg shadow-md space-y-4">
      <h2 className="text-lg font-bold text-gray-800 border-b pb-2">センサーデータ</h2>

      {/* 加速度 */}
      {sensorData.acceleration && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-1">加速度 [m/s²]（重力除く）</h3>
          <div className="grid grid-cols-3 gap-2">
            <AxisValue label="X" value={sensorData.acceleration.x} color="red" />
            <AxisValue label="Y" value={sensorData.acceleration.y} color="green" />
            <AxisValue label="Z" value={sensorData.acceleration.z} color="blue" />
          </div>
        </div>
      )}

      {/* 加速度（重力含む） */}
      {sensorData.accelerationIncludingGravity && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-1">加速度 [m/s²]（重力含む）</h3>
          <div className="grid grid-cols-3 gap-2">
            <AxisValue label="X" value={sensorData.accelerationIncludingGravity.x} color="red" />
            <AxisValue label="Y" value={sensorData.accelerationIncludingGravity.y} color="green" />
            <AxisValue label="Z" value={sensorData.accelerationIncludingGravity.z} color="blue" />
          </div>
        </div>
      )}

      {/* 回転角速度 */}
      {sensorData.rotationRate && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-1">回転角速度 [deg/s]</h3>
          <div className="grid grid-cols-3 gap-2">
            <AxisValue label="α" value={sensorData.rotationRate.alpha} color="purple" />
            <AxisValue label="β" value={sensorData.rotationRate.beta} color="purple" />
            <AxisValue label="γ" value={sensorData.rotationRate.gamma} color="purple" />
          </div>
        </div>
      )}

      {/* 端末の向き */}
      {sensorData.orientation && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-1">端末の向き [度]</h3>
          <div className="grid grid-cols-3 gap-2">
            <AxisValue label="α" value={sensorData.orientation.alpha} color="orange" />
            <AxisValue label="β" value={sensorData.orientation.beta} color="orange" />
            <AxisValue label="γ" value={sensorData.orientation.gamma} color="orange" />
          </div>
        </div>
      )}

      {/* タイムスタンプ */}
      <div className="text-xs text-gray-500 pt-2 border-t">
        タイムスタンプ: {new Date(sensorData.timestamp).toLocaleTimeString()}
      </div>
    </div>
  );
}

// 軸の値を表示するコンポーネント
interface AxisValueProps {
  label: string;
  value: number | null;
  color: "red" | "green" | "blue" | "purple" | "orange";
}

function AxisValue({ label, value, color }: AxisValueProps) {
  const colorClasses = {
    red: "bg-red-100 text-red-700",
    green: "bg-green-100 text-green-700",
    blue: "bg-blue-100 text-blue-700",
    purple: "bg-purple-100 text-purple-700",
    orange: "bg-orange-100 text-orange-700",
  };

  const barColorClasses = {
    red: "bg-red-500",
    green: "bg-green-500",
    blue: "bg-blue-500",
    purple: "bg-purple-500",
    orange: "bg-orange-500",
  };

  const displayValue = value !== null ? value.toFixed(2) : "N/A";

  // バーの幅を計算（-100〜100の範囲を0〜100%にマッピング）
  const barWidth = value !== null ? Math.min(Math.max((value + 100) / 2, 0), 100) : 50;

  return (
    <div className={`p-2 rounded ${colorClasses[color]}`}>
      <div className="text-xs font-semibold mb-1">{label}</div>
      <div className="text-lg font-mono">{displayValue}</div>
      <div className="mt-1 h-1 bg-gray-200 rounded overflow-hidden">
        <div
          className={`h-full ${barColorClasses[color]} transition-all duration-100`}
          style={{ width: `${barWidth}%` }}
        />
      </div>
    </div>
  );
}
