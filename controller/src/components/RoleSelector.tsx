"use client";

import type { PlayerRole, Team } from "@/lib/types";
import { OceanBackground } from "./OceanBackground";

interface RoleSelectorProps {
  onSelect: (role: PlayerRole) => void;
  disabledRoles?: PlayerRole[];
  team: Team;
}

const ROLE_CARDS: {
  value: PlayerRole;
  emoji: string;
  label: string;
  description: string;
}[] = [
  { value: "paddle_right", emoji: "🚣", label: "右オール", description: "スマホを振って漕ぐ！" },
  { value: "paddle_left", emoji: "🚣", label: "左オール", description: "スマホを振って漕ぐ！" },
  { value: "fisher", emoji: "🎣", label: "釣り", description: "スマホを振ってキャスト！" },
];

export function RoleSelector({ onSelect, disabledRoles = [], team }: RoleSelectorProps) {
  const teamLabel = team === "A" ? "チームA" : "チームB";
  const teamColor = team === "A" ? "from-blue-500 to-blue-600" : "from-red-500 to-red-600";

  return (
    <div className="relative min-h-screen flex flex-col items-center justify-center p-4">
      <OceanBackground team={team} />

      <div className="relative z-10 w-full max-w-sm">
        <div className="text-center mb-8 animate-slide-up">
          <div
            className={`inline-block px-4 py-1 rounded-full text-white text-sm font-bold bg-gradient-to-r ${teamColor} mb-3`}
          >
            {teamLabel}
          </div>
          <h1
            className="text-3xl font-black text-white"
            style={{ textShadow: "2px 2px 4px rgba(0,0,0,0.3)" }}
          >
            役割を選ぼう！
          </h1>
        </div>

        <div className="space-y-4">
          {ROLE_CARDS.map((role, index) => {
            const isDisabled = disabledRoles.includes(role.value);
            return (
              <button
                key={role.value}
                type="button"
                onClick={() => !isDisabled && onSelect(role.value)}
                disabled={isDisabled}
                className={`w-full p-5 rounded-2xl transition-all animate-slide-up ${
                  isDisabled
                    ? "bg-white/20 backdrop-blur-sm cursor-not-allowed opacity-60"
                    : "bg-white/90 backdrop-blur-sm shadow-xl hover:scale-[1.02] active:scale-95 border-2 border-transparent hover:border-white/50"
                }`}
                style={{ animationDelay: `${index * 0.1}s` }}
              >
                <div className="flex items-center gap-4">
                  <div className="text-4xl">{role.emoji}</div>
                  <div className="flex-1 text-left">
                    <div className="text-lg font-black text-gray-800">{role.label}</div>
                    <div className="text-sm text-gray-600">{role.description}</div>
                  </div>
                  {isDisabled ? (
                    <span className="text-xs bg-red-100 text-red-600 px-3 py-1 rounded-full font-bold">
                      使用中
                    </span>
                  ) : (
                    <div
                      className={`w-8 h-8 rounded-full bg-gradient-to-r ${teamColor} flex items-center justify-center`}
                    >
                      <span className="text-white text-lg">→</span>
                    </div>
                  )}
                </div>
              </button>
            );
          })}
        </div>

        <p className="text-center text-white/60 text-xs mt-6 font-medium">
          ※ 同じチーム内で各役割は1人まで
        </p>
      </div>
    </div>
  );
}
