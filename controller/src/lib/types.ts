export type PlayerRole = "paddle_right" | "paddle_left" | "fisher";

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
    description: "スマホを向けて狙い、キャスト/引き上げ",
  },
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
}

export interface ControllerSensorPayload {
  roomId?: string;
  role: PlayerRole;
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
