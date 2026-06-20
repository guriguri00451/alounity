"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

export default function Home() {
  const router = useRouter();
  const [roomId, setRoomId] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const trimmed = roomId.trim().toUpperCase();

    if (!trimmed) {
      setError("ルームIDを入力してください");
      return;
    }

    if (!/^[A-Z2-9]+$/.test(trimmed)) {
      setError("使用できない文字が含まれています（I, O, 0, 1は使用できません）");
      return;
    }

    router.push(`/room/${trimmed}`);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRoomId(e.target.value.toUpperCase().replace(/[^A-Z2-9]/g, ""));
    setError("");
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gradient-to-b from-blue-50 to-blue-100">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">カヤックコントローラー</h1>
          <p className="text-gray-600 text-sm">ルームIDを入力して参加</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="bg-white rounded-lg shadow-md p-6">
            <label htmlFor="roomId" className="block text-sm font-semibold text-gray-700 mb-2">
              ルームID
            </label>
            <input
              id="roomId"
              type="text"
              value={roomId}
              onChange={handleChange}
              placeholder="例: ABC123"
              maxLength={6}
              className="w-full text-center text-2xl tracking-[0.5em] font-mono px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-transparent uppercase placeholder:text-gray-300 placeholder:tracking-normal"
              autoComplete="off"
            />
            <p className="text-xs text-gray-500 mt-2 text-center">
              英数字6文字（I, O, 0, 1は不使用）
            </p>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg p-3 text-center">
              {error}
            </div>
          )}

          <button
            type="submit"
            className="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-lg shadow-md transition-colors text-lg"
          >
            参加する
          </button>
        </form>
      </div>
    </div>
  );
}
