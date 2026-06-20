"use client";

import { useEffect, useMemo, useState } from "react";
import type { SensorData } from "@/hooks/useDeviceMotion";
import type { Team } from "@/lib/types";

interface FisherVisualProps {
  sensorData: SensorData | null;
  team: Team;
}

const SWING_THRESHOLD = 5;

export function FisherVisual({ sensorData, team }: FisherVisualProps) {
  const [isSwinging, setIsSwinging] = useState(false);

  const swingForce = useMemo(() => {
    if (!sensorData?.acceleration) return 0;
    const { x, y, z } = sensorData.acceleration;
    return Math.sqrt((x ?? 0) ** 2 + (y ?? 0) ** 2 + (z ?? 0) ** 2);
  }, [sensorData]);

  const normalizedForce = Math.min(Math.max((swingForce - SWING_THRESHOLD) / 15, 0), 1);

  useEffect(() => {
    if (swingForce > SWING_THRESHOLD) {
      setIsSwinging(true);
      const timer = setTimeout(() => setIsSwinging(false), 400);
      return () => clearTimeout(timer);
    }
  }, [swingForce]);

  const teamColor = team === "A" ? "#3B82F6" : "#EF4444";
  const teamColorLight = team === "A" ? "#93C5FD" : "#FCA5A5";

  const rodBend = isSwinging ? -35 : 0;
  const rodSwing = isSwinging ? 15 : 0;

  return (
    <div className="relative flex flex-col items-center">
      <div
        className="transition-transform origin-bottom"
        style={{
          transform: `rotate(${rodSwing}deg)`,
          transitionDuration: isSwinging ? "150ms" : "400ms",
          transitionTimingFunction: isSwinging ? "ease-out" : "ease-in",
        }}
      >
        <svg
          width="160"
          height="260"
          viewBox="0 0 160 260"
          fill="none"
          role="img"
          aria-label="釣り竿のイラスト"
        >
          <defs>
            <linearGradient id="rod-gradient" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stopColor="#4A4A4A" />
              <stop offset="100%" stopColor="#2A2A2A" />
            </linearGradient>
            <linearGradient id="handle-gradient" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" stopColor={teamColor} />
              <stop offset="100%" stopColor={teamColorLight} />
            </linearGradient>
          </defs>

          <path
            d={`M80,240 Q80,190 80,140 Q80,100 ${85 + rodBend * 0.3},${70 + rodBend * 0.5} Q${87 + rodBend * 0.5},${40 + rodBend} ${90 + rodBend * 0.7},${20 + rodBend * 1.2}`}
            stroke="url(#rod-gradient)"
            strokeWidth="6"
            strokeLinecap="round"
            fill="none"
            className="transition-all"
            style={{ transitionDuration: isSwinging ? "150ms" : "400ms" }}
          />
          <path
            d={`M80,240 Q80,190 80,140 Q80,100 ${85 + rodBend * 0.3},${70 + rodBend * 0.5} Q${87 + rodBend * 0.5},${40 + rodBend} ${90 + rodBend * 0.7},${20 + rodBend * 1.2}`}
            stroke="white"
            strokeOpacity="0.2"
            strokeWidth="2"
            strokeLinecap="round"
            fill="none"
            className="transition-all"
            style={{ transitionDuration: isSwinging ? "150ms" : "400ms" }}
          />

          <circle
            cx={90 + rodBend * 0.7}
            cy={20 + rodBend * 1.2}
            r="3"
            fill="#FFD700"
            className="transition-all"
            style={{ transitionDuration: isSwinging ? "150ms" : "400ms" }}
          />

          <path
            d={`M${90 + rodBend * 0.7},${20 + rodBend * 1.2} Q${100 + rodBend * 0.5},${40 + rodBend * 0.8} ${110 + rodBend * 0.3},${70 + rodBend * 0.5} Q${115},${100} ${112},${140}`}
            stroke="white"
            strokeOpacity="0.6"
            strokeWidth="1"
            strokeDasharray="4,4"
            fill="none"
            className="transition-all"
            style={{ transitionDuration: isSwinging ? "150ms" : "400ms" }}
          />

          <circle
            cx={112}
            cy={140}
            r="6"
            fill="#FF6B35"
            stroke="white"
            strokeWidth="2"
            className={isSwinging ? "animate-bounce" : ""}
          />
          <circle cx={112} cy={140} r="3" fill="white" fillOpacity="0.5" />

          <rect x="72" y="210" width="16" height="40" rx="4" fill="url(#handle-gradient)" />
          <rect x="70" y="205" width="20" height="8" rx="2" fill="#333" />
          <rect x="70" y="248" width="20" height="8" rx="2" fill="#333" />

          <circle cx="80" cy="195" r="12" fill="#555" stroke="#333" strokeWidth="2" />
          <circle cx="80" cy="195" r="6" fill="#777" />
          <circle cx="80" cy="195" r="2" fill="#333" />
        </svg>
      </div>

      <div className="mt-4 text-center">
        <div
          className="text-4xl font-black transition-all"
          style={{
            color: normalizedForce > 0.3 ? "#FFD700" : "white",
            textShadow: "2px 2px 4px rgba(0,0,0,0.3)",
            transform: `scale(${1 + normalizedForce * 0.3})`,
          }}
        >
          {Math.round(normalizedForce * 100)}
        </div>
        <div className="text-white/80 text-sm font-bold">POWER</div>
      </div>

      {isSwinging && (
        <div className="absolute top-8 left-1/2 -translate-x-1/2 flex gap-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <div
              // biome-ignore lint/suspicious/noArrayIndexKey: ephemeral spark animation
              key={`spark-${i}`}
              className="w-2 h-2 rounded-full bg-yellow-300 animate-splash"
              style={{ animationDelay: `${i * 0.05}s` }}
            />
          ))}
        </div>
      )}
    </div>
  );
}
