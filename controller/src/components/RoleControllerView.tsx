"use client";

import type { SensorData } from "@/hooks/useDeviceMotion";
import type { PlayerRole, Team } from "@/lib/types";
import { FisherVisual } from "./FisherVisual";
import { PaddleVisual } from "./PaddleVisual";

interface RoleControllerViewProps {
  role: PlayerRole;
  team: Team;
  sensorData: SensorData | null;
  isConnected: boolean;
  isListening: boolean;
  roomId: string;
  gameMode: "single" | "versus";
  onToggleListening: () => void;
}

const ROLE_CONFIG: Record<PlayerRole, { label: string; emoji: string; instruction: string }> = {
  paddle_right: {
    label: "右オール",
    emoji: "🛶",
    instruction: "スマホを振ってカヤックを進め！",
  },
  paddle_left: {
    label: "左オール",
    emoji: "🛶",
    instruction: "スマホを振ってカヤックを進め！",
  },
  fisher: {
    label: "釣り",
    emoji: "🎣",
    instruction: "方位磁針で狙ってキャスト！",
  },
};

export function RoleControllerView({
  role,
  team,
  sensorData,
  isConnected,
  isListening,
  roomId,
  gameMode,
  onToggleListening,
}: RoleControllerViewProps) {
  const config = ROLE_CONFIG[role];
  const teamLabel = team === "A" ? "チームA" : "チームB";
  const teamBadge = team === "A" ? "bg-blue-500" : "bg-red-500";

  return (
    <div className="relative min-h-screen flex flex-col items-center justify-between p-4 overflow-hidden">
      <header className="relative z-10 w-full text-center pt-2">
        <div className="inline-flex items-center gap-2 bg-white/20 backdrop-blur-sm rounded-full px-4 py-2">
          <span className="text-white/80 text-xs font-bold">ROOM</span>
          <span className="text-white font-mono font-bold text-sm">{roomId}</span>
          {gameMode === "versus" && (
            <span className={`${teamBadge} text-white text-xs font-bold px-2 py-0.5 rounded-full`}>
              {teamLabel}
            </span>
          )}
        </div>
      </header>

      <div className="relative z-10 flex-1 flex flex-col items-center justify-center">
        <div className="text-center mb-6 animate-slide-up">
          <div className="text-5xl mb-2">{config.emoji}</div>
          <h1
            className="text-3xl font-black text-white"
            style={{ textShadow: "2px 2px 4px rgba(0,0,0,0.3)" }}
          >
            {config.label}
          </h1>
          <p className="text-white/80 text-sm mt-1 font-medium">{config.instruction}</p>
        </div>

        <div className="animate-float" style={{ animationDuration: "4s" }}>
          {role === "paddle_right" && (
            <PaddleVisual sensorData={sensorData} side="right" team={team} />
          )}
          {role === "paddle_left" && (
            <PaddleVisual sensorData={sensorData} side="left" team={team} />
          )}
          {role === "fisher" && <FisherVisual sensorData={sensorData} team={team} />}
        </div>
      </div>

      <footer className="relative z-10 w-full max-w-sm space-y-3 pb-4">
        <div className="flex items-center justify-center gap-2">
          <div
            className={`w-3 h-3 rounded-full ${isConnected ? "bg-green-400" : "bg-red-400"}`}
            style={isConnected ? { boxShadow: "0 0 8px #4ade80" } : {}}
          />
          <span className="text-white/80 text-xs font-bold">
            {isConnected ? "接続中" : "未接続"}
          </span>
        </div>

        <button
          type="button"
          onClick={onToggleListening}
          className={`w-full py-3 rounded-full font-bold text-sm transition-all active:scale-95 ${
            isListening
              ? "bg-white/30 text-white backdrop-blur-sm border-2 border-white/50"
              : "bg-white text-gray-800 shadow-lg"
          }`}
        >
          {isListening ? "⏸ 一時停止" : "▶ 再開"}
        </button>
      </footer>
    </div>
  );
}
