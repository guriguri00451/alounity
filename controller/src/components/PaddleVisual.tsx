"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";
import type { SensorData } from "@/hooks/useDeviceMotion";
import type { Team } from "@/lib/types";

interface PaddleVisualProps {
  sensorData: SensorData | null;
  side: "right" | "left";
  team: Team;
}

const SWING_THRESHOLD = 30;
const POWER_HOLD_DURATION = 800;

export function PaddleVisual({ sensorData, side, team }: PaddleVisualProps) {
  const uid = useId();
  const [isStroking, setIsStroking] = useState(false);
  const [displayedPower, setDisplayedPower] = useState(0);
  const powerHoldTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const magnitude = useMemo(() => {
    if (!sensorData?.acceleration) return 0;
    const { x, y, z } = sensorData.acceleration;
    return Math.sqrt((x ?? 0) ** 2 + (y ?? 0) ** 2 + (z ?? 0) ** 2);
  }, [sensorData]);

  const normalizedForce = Math.min(Math.max((magnitude - SWING_THRESHOLD) / 15, 0), 1);

  useEffect(() => {
    if (magnitude > SWING_THRESHOLD) {
      setIsStroking(true);
    } else if (isStroking) {
      const timer = setTimeout(() => setIsStroking(false), 300);
      return () => clearTimeout(timer);
    }
  }, [magnitude, isStroking]);

  useEffect(() => {
    setDisplayedPower((prev) => Math.max(prev, normalizedForce));

    if (normalizedForce > 0) {
      if (powerHoldTimerRef.current) {
        clearTimeout(powerHoldTimerRef.current);
      }
      powerHoldTimerRef.current = setTimeout(() => {
        setDisplayedPower(0);
      }, POWER_HOLD_DURATION);
    }
  }, [normalizedForce]);

  useEffect(() => {
    return () => {
      if (powerHoldTimerRef.current) {
        clearTimeout(powerHoldTimerRef.current);
      }
    };
  }, []);

  const rotation = isStroking ? (side === "right" ? 25 : -25) : 0;
  const teamColor = team === "A" ? "#3B82F6" : "#EF4444";
  const teamColorLight = team === "A" ? "#93C5FD" : "#FCA5A5";

  return (
    <div className="relative flex flex-col items-center">
      <div
        className="transition-transform"
        style={{
          transform: `rotate(${rotation}deg) scale(${1 + (isStroking ? normalizedForce * 0.1 : 0)})`,
          transitionDuration: isStroking ? "150ms" : "300ms",
          transitionTimingFunction: isStroking ? "ease-out" : "ease-in-out",
        }}
      >
        <svg
          width="200"
          height="280"
          viewBox="0 0 200 280"
          fill="none"
          role="img"
          aria-label={`${side === "right" ? "右" : "左"}オールのイラスト`}
        >
          <defs>
            <linearGradient id={`paddle-blade-${side}`} x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stopColor={teamColor} />
              <stop offset="100%" stopColor={teamColorLight} />
            </linearGradient>
            <linearGradient id={`shaft-${side}`} x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" stopColor="#8B5E3C" />
              <stop offset="100%" stopColor="#6B4423" />
            </linearGradient>
          </defs>

          <rect x="95" y="60" width="10" height="160" rx="5" fill={`url(#shaft-${side})`} />

          <ellipse
            cx="100"
            cy="45"
            rx="35"
            ry="50"
            fill={`url(#paddle-blade-${side})`}
            stroke={teamColor}
            strokeWidth="2"
          />
          <ellipse cx="100" cy="45" rx="20" ry="35" fill="white" fillOpacity="0.2" />

          <ellipse
            cx="100"
            cy="235"
            rx="35"
            ry="50"
            fill={`url(#paddle-blade-${side})`}
            stroke={teamColor}
            strokeWidth="2"
          />
          <ellipse cx="100" cy="235" rx="20" ry="35" fill="white" fillOpacity="0.2" />

          <circle cx="100" cy="140" r="8" fill="#D4A574" stroke="#8B5E3C" strokeWidth="2" />
        </svg>
      </div>

      {isStroking && normalizedForce > 0.1 && (
        <div className="absolute bottom-0 left-1/2 -translate-x-1/2 flex gap-1">
          {Array.from({ length: Math.ceil(normalizedForce * 5) }).map((_, i) => (
            <div
              // biome-ignore lint/suspicious/noArrayIndexKey: ephemeral splash animation
              key={`${uid}-splash-${i}`}
              className="w-2 h-2 rounded-full bg-white/60 animate-splash"
              style={{ animationDelay: `${i * 0.1}s` }}
            />
          ))}
        </div>
      )}

      <div className="mt-4 text-center">
        <div
          className="text-4xl font-black transition-all"
          style={{
            color: displayedPower > 0.3 ? "#FFD700" : "white",
            textShadow: "2px 2px 4px rgba(0,0,0,0.3)",
            transform: `scale(${1 + displayedPower * 0.3})`,
          }}
        >
          {Math.round(displayedPower * 100)}
        </div>
        <div className="text-white/80 text-sm font-bold">POWER</div>
      </div>
    </div>
  );
}
