"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { io } from "socket.io-client";

export default function Home() {
  const router = useRouter();
  const [roomId, setRoomId] = useState("");
  const [error, setError] = useState("");
  const [isChecking, setIsChecking] = useState(false);

  const checkRoomExists = (id: string): Promise<boolean> => {
    return new Promise((resolve) => {
      const serverUrl = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
      const socket = io(serverUrl, {
        transports: ["websocket", "polling"],
        reconnection: false,
      });

      const timeout = setTimeout(() => {
        socket.disconnect();
        resolve(false);
      }, 5000);

      socket.on("connect", () => {
        socket.emit("room:exists", { roomId: id });
      });

      socket.on("room:exists_ack", (data) => {
        clearTimeout(timeout);
        socket.disconnect();
        resolve(data.exists === true);
      });

      socket.on("connect_error", () => {
        clearTimeout(timeout);
        socket.disconnect();
        resolve(false);
      });
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
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

    setIsChecking(true);
    setError("");

    const exists = await checkRoomExists(trimmed);
    setIsChecking(false);

    if (!exists) {
      setError("ルームが見つかりません");
      return;
    }

    router.push(`/room/${trimmed}`);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRoomId(
      e.target.value
        .toUpperCase()
        .replace(/[^A-Z2-9]/g, "")
        .slice(0, 6)
    );
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
              disabled={isChecking}
              className="w-full text-center text-2xl tracking-[0.5em] font-mono px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-transparent uppercase placeholder:text-gray-300 placeholder:tracking-normal disabled:bg-gray-100 disabled:text-gray-400"
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
            disabled={isChecking}
            className="w-full py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-bold rounded-lg shadow-md transition-colors text-lg"
          >
            {isChecking ? "確認中..." : "参加する"}
          </button>
        </form>
      </div>
    </div>
  );
}
