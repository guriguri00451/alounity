"use client";

import type { Team } from "@/lib/types";
import { TEAMS } from "@/lib/types";

interface TeamSelectorProps {
  onSelect: (team: Team) => void;
  disabledTeams?: Team[];
}

export function TeamSelector({ onSelect, disabledTeams = [] }: TeamSelectorProps) {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-2xl font-bold text-gray-800 mb-2">チームを選択</h1>
          <p className="text-gray-600 text-sm">参加するチームを選んでください</p>
        </div>

        <div className="space-y-4">
          {TEAMS.map((team) => {
            const isDisabled = disabledTeams.includes(team.value);
            const bgColor =
              team.value === "A" ? "bg-blue-500 hover:bg-blue-600" : "bg-red-500 hover:bg-red-600";
            const disabledBg = "bg-gray-300 cursor-not-allowed";

            return (
              <button
                key={team.value}
                type="button"
                onClick={() => !isDisabled && onSelect(team.value)}
                disabled={isDisabled}
                className={`w-full p-6 rounded-lg shadow-md transition-shadow text-white font-bold text-xl ${
                  isDisabled ? disabledBg : bgColor
                }`}
              >
                <div className="flex items-center justify-between">
                  <span>{team.label}</span>
                  {isDisabled && (
                    <span className="text-xs bg-white/30 px-2 py-1 rounded">満席</span>
                  )}
                </div>
              </button>
            );
          })}
        </div>

        <p className="text-center text-xs text-gray-500 mt-8">
          ※ 各チーム3人まで（右オール・左オール・釣り）
        </p>
      </div>
    </div>
  );
}
