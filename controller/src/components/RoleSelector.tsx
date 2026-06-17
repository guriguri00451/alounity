"use client";

import type { PlayerRole } from "@/lib/types";
import { PLAYER_ROLES } from "@/lib/types";

interface RoleSelectorProps {
  onSelect: (role: PlayerRole) => void;
}

export function RoleSelector({ onSelect }: RoleSelectorProps) {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-2xl font-bold text-gray-800 mb-2">カヤックコントローラー</h1>
          <p className="text-gray-600 text-sm">役割を選択してください</p>
        </div>

        <div className="space-y-4">
          {PLAYER_ROLES.map((role) => (
            <button
              key={role.value}
              type="button"
              onClick={() => onSelect(role.value)}
              className="w-full p-4 bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow text-left"
            >
              <div className="text-lg font-bold text-gray-800">{role.label}</div>
              <div className="text-sm text-gray-600 mt-1">{role.description}</div>
            </button>
          ))}
        </div>

        <p className="text-center text-xs text-gray-500 mt-8">
          ※ 同じチーム内で各役割は1人ずつです
        </p>
      </div>
    </div>
  );
}
