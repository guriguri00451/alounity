export type PlayerRole = "paddle_right" | "paddle_left" | "fisher";

export type Team = "A" | "B";

export type GameMode = "single" | "versus";

export const PLAYER_ROLES: { value: PlayerRole; label: string; description: string }[] = [
  {
    value: "paddle_right",
    label: "右オール",
    description: "スマホを振ってカヤック右側を漕ぐ",
  },
  {
    value: "paddle_left",
    label: "左オール",
    description: "スマホを振ってカヤック左側を漕ぐ",
  },
  {
    value: "fisher",
    label: "釣り",
    description: "スマホを振ってキャスト/引き上げ",
  },
];

export const TEAMS: { value: Team; label: string; color: string }[] = [
  { value: "A", label: "チームA", color: "blue" },
  { value: "B", label: "チームB", color: "red" },
];

export interface AccelData {
  x: number;
  y: number;
  z: number;
}

export interface RotationData {
  alpha: number;
  beta: number;
  gamma: number;
}

export interface OrientationData {
  alpha: number;
  beta: number;
  gamma: number;
}

export interface SensorPayload {
  accel: AccelData | null;
  rotation: RotationData | null;
  orientation: OrientationData | null;
  timestamp: number;
}

export interface ControllerConnectPayload {
  roomId?: string;
  role: PlayerRole;
  team: Team;
}

export interface ControllerSensorPayload {
  roomId?: string;
  role: PlayerRole;
  team: Team;
  accel: AccelData | null;
  rotation: RotationData | null;
  orientation: OrientationData | null;
  timestamp: number;
}

export interface ServerAckPayload {
  received: boolean;
  playerId?: string;
  error?: string;
}

export interface PlayerInfo {
  playerId: string;
  role: PlayerRole;
  team: Team;
}
