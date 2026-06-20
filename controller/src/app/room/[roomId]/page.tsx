"use client";

import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { io } from "socket.io-client";
import { OceanBackground } from "@/components/OceanBackground";
import { PermissionRequest } from "@/components/PermissionRequest";
import { RoleControllerView } from "@/components/RoleControllerView";
import { RoleSelector } from "@/components/RoleSelector";
import { SensorDebugOverlay } from "@/components/SensorDebugOverlay";
import { TeamSelector } from "@/components/TeamSelector";
import { useDeviceMotion } from "@/hooks/useDeviceMotion";
import { useSocket } from "@/hooks/useSocket";
import type { GameMode, PlayerInfo, PlayerRole, Team } from "@/lib/types";

const ALL_ROLES: PlayerRole[] = ["paddle_right", "paddle_left", "fisher"];

export default function RoomPage() {
  const params = useParams();
  const roomId = typeof params.roomId === "string" ? params.roomId : "";

  const [team, setTeam] = useState<Team | null>(null);
  const [role, setRole] = useState<PlayerRole | null>(null);
  const [hasPermission, setHasPermission] = useState(false);
  const [isListening, setIsListening] = useState(false);
  const [players, setPlayers] = useState<PlayerInfo[]>([]);
  const [rolesLoading, setRolesLoading] = useState(true);
  const [gameMode, setGameMode] = useState<GameMode>("single");

  const { sensorData, isSupported, startListening, stopListening } = useDeviceMotion({
    throttleMs: 33,
  });

  const { isConnected, connectionError, sendSensorData } = useSocket({
    roomId,
    role: role ?? "paddle_right",
    team: team ?? "A",
    autoConnect: role !== null && team !== null,
  });

  // singleモードではチームAに自動設定
  useEffect(() => {
    if (gameMode === "single" && team === null) {
      setTeam("A");
    }
  }, [gameMode, team]);

  // takenRoles（チーム別）からplayersを構築
  const setTakenRolesFromData = useCallback((takenRoles: Record<string, string[]>) => {
    const playerList: PlayerInfo[] = [];
    for (const [t, roles] of Object.entries(takenRoles)) {
      for (const r of roles) {
        playerList.push({ playerId: "", role: r as PlayerRole, team: t as Team });
      }
    }
    setPlayers(playerList);
  }, []);

  // ルーム情報をリアルタイム取得（チーム・役割選択前のみ接続）
  useEffect(() => {
    if (!roomId || (role && team)) return;

    const serverUrl = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
    const socket = io(serverUrl, {
      transports: ["websocket", "polling"],
      reconnection: false,
    });

    const timeout = setTimeout(() => {
      socket.disconnect();
      setRolesLoading(false);
    }, 5000);

    socket.on("connect", () => {
      socket.emit("room:exists", { roomId });
    });

    socket.on("room:exists_ack", (data) => {
      clearTimeout(timeout);
      setRolesLoading(false);

      if (data.exists) {
        if (data.gameMode) setGameMode(data.gameMode as GameMode);
        if (data.takenRoles) setTakenRolesFromData(data.takenRoles);
      }
    });

    socket.on("room:players_update", (data) => {
      if (data.players) {
        setPlayers(data.players as PlayerInfo[]);
      }
    });

    socket.on("connect_error", () => {
      clearTimeout(timeout);
      socket.disconnect();
      setRolesLoading(false);
    });

    return () => {
      clearTimeout(timeout);
      socket.disconnect();
    };
  }, [roomId, role, team, setTakenRolesFromData]);

  // 接続エラー時に役割選択画面に戻る
  useEffect(() => {
    if (connectionError === "この役割は既に使用されています") {
      setRole(null);
    }
  }, [connectionError]);

  useEffect(() => {
    if (!sensorData || !isListening || !isConnected) return;

    sendSensorData({
      accel: sensorData.acceleration
        ? {
            x: sensorData.acceleration.x ?? 0,
            y: sensorData.acceleration.y ?? 0,
            z: sensorData.acceleration.z ?? 0,
          }
        : null,
      rotation: sensorData.rotationRate
        ? {
            alpha: sensorData.rotationRate.alpha ?? 0,
            beta: sensorData.rotationRate.beta ?? 0,
            gamma: sensorData.rotationRate.gamma ?? 0,
          }
        : null,
      orientation: sensorData.orientation
        ? {
            alpha: sensorData.orientation.alpha ?? 0,
            beta: sensorData.orientation.beta ?? 0,
            gamma: sensorData.orientation.gamma ?? 0,
          }
        : null,
      timestamp: sensorData.timestamp,
    });
  }, [sensorData, isListening, isConnected, sendSensorData]);

  const handlePermissionGranted = useCallback(() => {
    setHasPermission(true);
    setIsListening(true);
    startListening();
  }, [startListening]);

  const handlePermissionDenied = useCallback(() => {
    console.log("Permission denied");
  }, []);

  const handleToggleListening = useCallback(() => {
    if (isListening) {
      setIsListening(false);
      stopListening();
    } else {
      setIsListening(true);
      startListening();
    }
  }, [isListening, startListening, stopListening]);

  // 現在のチームで占有されている役割
  const takenRolesInTeam = players.filter((p) => p.team === (team ?? "A")).map((p) => p.role);

  // チーム別の占有数
  const teamACount = players.filter((p) => p.team === "A").length;
  const teamBCount = players.filter((p) => p.team === "B").length;
  const disabledTeams: Team[] = [];
  if (teamACount >= ALL_ROLES.length) disabledTeams.push("A");
  if (teamBCount >= ALL_ROLES.length) disabledTeams.push("B");

  if (!isSupported) {
    return (
      <div className="relative min-h-screen flex items-center justify-center p-4">
        <OceanBackground />
        <div className="relative z-10 max-w-sm w-full bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-6 text-center">
          <div className="text-5xl mb-4">📱</div>
          <h1 className="text-xl font-black text-gray-800 mb-2">未対応ブラウザ</h1>
          <p className="text-gray-600 text-sm">このブラウザはセンサーAPIをサポートしていません。</p>
          <p className="text-xs text-gray-500 mt-3">
            iOS Safari（13+）またはChrome（Android）を使用してください。
          </p>
        </div>
      </div>
    );
  }

  if (rolesLoading) {
    return (
      <div className="relative min-h-screen flex items-center justify-center p-4">
        <OceanBackground />
        <div className="relative z-10 text-center">
          <div className="text-6xl animate-bounce mb-4">🛶</div>
          <p
            className="text-white font-bold text-lg"
            style={{ textShadow: "2px 2px 4px rgba(0,0,0,0.3)" }}
          >
            ルームに接続中...
          </p>
        </div>
      </div>
    );
  }

  // versus モードの場合はチーム選択が必要
  if (gameMode === "versus" && !team) {
    const allFull = disabledTeams.length >= 2;
    if (allFull) {
      return (
        <div className="relative min-h-screen flex items-center justify-center p-4">
          <OceanBackground />
          <div className="relative z-10 max-w-sm w-full bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-6 text-center">
            <div className="text-5xl mb-4">😵</div>
            <h1 className="text-xl font-black text-red-600 mb-2">満席です</h1>
            <p className="text-gray-600 text-sm">
              両チームとも満席です。別のルームに参加してください。
            </p>
          </div>
        </div>
      );
    }
    return <TeamSelector onSelect={setTeam} disabledTeams={disabledTeams} />;
  }

  // 役割選択
  if (!role) {
    const currentTeam = team ?? "A";
    const allTaken = takenRolesInTeam.length >= ALL_ROLES.length;
    if (allTaken) {
      return (
        <div className="relative min-h-screen flex items-center justify-center p-4">
          <OceanBackground team={currentTeam} />
          <div className="relative z-10 max-w-sm w-full bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-6 text-center">
            <div className="text-5xl mb-4">😵</div>
            <h1 className="text-xl font-black text-red-600 mb-2">満席です</h1>
            <p className="text-gray-600 text-sm">
              {currentTeam === "A" ? "チームA" : "チームB"}は全ての役割が使用中です。
            </p>
          </div>
        </div>
      );
    }
    return <RoleSelector onSelect={setRole} disabledRoles={takenRolesInTeam} team={currentTeam} />;
  }

  if (!hasPermission) {
    return (
      <PermissionRequest
        onPermissionGranted={handlePermissionGranted}
        onPermissionDenied={handlePermissionDenied}
      />
    );
  }

  return (
    <>
      <OceanBackground team={team ?? undefined} />
      <SensorDebugOverlay sensorData={sensorData} isConnected={isConnected} role={role} />
      <RoleControllerView
        role={role}
        team={team ?? "A"}
        sensorData={sensorData}
        isConnected={isConnected}
        isListening={isListening}
        roomId={roomId}
        gameMode={gameMode}
        onToggleListening={handleToggleListening}
      />
    </>
  );
}
