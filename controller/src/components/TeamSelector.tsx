"use client";

import type { Team } from "@/lib/types";
import { OceanBackground } from "./OceanBackground";

interface TeamSelectorProps {
  onSelect: (team: Team) => void;
  disabledTeams?: Team[];
}

const TEAM_CARDS: { value: Team; label: string; emoji: string; gradient: string }[] = [
  { value: "A", label: "チームA", emoji: "🔵", gradient: "from-blue-400 to-blue-600" },
  { value: "B", label: "チームB", emoji: "🔴", gradient: "from-red-400 to-red-600" },
];

export function TeamSelector({ onSelect, disabledTeams = [] }: TeamSelectorProps) {
  return (
    <div className="relative min-h-screen flex flex-col items-center justify-center p-4">
      <OceanBackground />

      <div className="relative z-10 w-full max-w-sm">
        <div className="text-center mb-8 animate-slide-up">
          <div className="text-5xl mb-3">⚔️</div>
          <h1
            className="text-3xl font-black text-white"
            style={{ textShadow: "2px 2px 4px rgba(0,0,0,0.3)" }}
          >
            チームを選ぼう！
          </h1>
          <p className="text-white/70 text-sm mt-2 font-medium">参加するチームを選んでください</p>
        </div>

        <div className="space-y-4">
          {TEAM_CARDS.map((team, index) => {
            const isDisabled = disabledTeams.includes(team.value);
            return (
              <button
                key={team.value}
                type="button"
                onClick={() => !isDisabled && onSelect(team.value)}
                disabled={isDisabled}
                className={`w-full p-6 rounded-2xl transition-all animate-slide-up ${
                  isDisabled
                    ? "bg-white/20 backdrop-blur-sm cursor-not-allowed opacity-60"
                    : `bg-gradient-to-r ${team.gradient} shadow-xl hover:scale-[1.02] active:scale-95`
                }`}
                style={{ animationDelay: `${index * 0.15}s` }}
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <span className="text-3xl">{team.emoji}</span>
                    <span className="text-white text-xl font-black">{team.label}</span>
                  </div>
                  {isDisabled ? (
                    <span className="text-xs bg-white/30 text-white px-3 py-1 rounded-full font-bold">
                      満席
                    </span>
                  ) : (
                    <div className="w-10 h-10 rounded-full bg-white/30 flex items-center justify-center">
                      <span className="text-white text-2xl">→</span>
                    </div>
                  )}
                </div>
              </button>
            );
          })}
        </div>

        <p className="text-center text-white/60 text-xs mt-6 font-medium">
          ※ 各チーム3人まで（右オール・左オール・釣り）
        </p>
      </div>
    </div>
  );
}
