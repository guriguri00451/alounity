"use client";

import type { PlayerRole, Team } from "@/lib/types";
import { PLAYER_ROLES } from "@/lib/types";

interface RoleSelectorProps {
  onSelect: (role: PlayerRole) => void;
  disabledRoles?: PlayerRole[];
  team: Team;
}

export function RoleSelector({ onSelect, disabledRoles = [], team }: RoleSelectorProps) {
  const teamLabel = team === "A" ? "チームA" : "チームB";
  const teamColor = team === "A" ? "text-blue-600" : "text-red-600";

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-2xl font-bold text-gray-800 mb-2">役割を選択</h1>
          <p className={`text-sm font-semibold ${teamColor}`}>{teamLabel}</p>
        </div>

        <div className="space-y-4">
          {PLAYER_ROLES.map((role) => {
            const isDisabled = disabledRoles.includes(role.value);
            return (
              <button
                key={role.value}
                type="button"
                onClick={() => !isDisabled && onSelect(role.value)}
                disabled={isDisabled}
                className={`w-full p-4 rounded-lg shadow-md transition-shadow text-left ${
                  isDisabled
                    ? "bg-gray-200 opacity-60 cursor-not-allowed"
                    : "bg-white hover:shadow-lg"
                }`}
              >
                <div className="flex items-center justify-between">
                  <div className="text-lg font-bold text-gray-800">{role.label}</div>
                  {isDisabled && (
                    <span className="text-xs bg-red-100 text-red-600 px-2 py-1 rounded">
                      使用中
                    </span>
                  )}
                </div>
                <div className="text-sm text-gray-600 mt-1">{role.description}</div>
              </button>
            );
          })}
        </div>

        <p className="text-center text-xs text-gray-500 mt-8">
          ※ 同じチーム内で各役割は1人ずつです
        </p>
      </div>
    </div>
  );
}
