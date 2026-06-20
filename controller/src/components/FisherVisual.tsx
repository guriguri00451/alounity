"use client";

import { useMemo } from "react";
import type { SensorData } from "@/hooks/useDeviceMotion";
import type { Team } from "@/lib/types";

interface FisherVisualProps {
  sensorData: SensorData | null;
  team: Team;
}

export function FisherVisual({ sensorData, team }: FisherVisualProps) {
  const direction = useMemo(() => {
    if (!sensorData?.orientation) return 0;
    return sensorData.orientation.alpha ?? 0;
  }, [sensorData]);

  const castForce = useMemo(() => {
    if (!sensorData?.rotationRate) return 0;
    const beta = sensorData.rotationRate.beta ?? 0;
    return Math.min(Math.abs(beta) / 180, 1);
  }, [sensorData]);

  const teamColor = team === "A" ? "#3B82F6" : "#EF4444";
  const teamColorLight = team === "A" ? "#93C5FD" : "#FCA5A5";

  const rodBend = castForce * 20;

  return (
    <div className="relative flex flex-col items-center">
      <div className="relative">
        <svg
          width="180"
          height="60"
          viewBox="0 0 180 60"
          fill="none"
          className="mb-2"
          role="img"
          aria-label="方位磁針"
        >
          <circle
            cx="90"
            cy="30"
            r="28"
            fill="white"
            fillOpacity="0.2"
            stroke="white"
            strokeOpacity="0.5"
            strokeWidth="2"
          />
          <circle cx="90" cy="30" r="22" fill="white" fillOpacity="0.1" />

          <text
            x="90"
            y="14"
            textAnchor="middle"
            fill="white"
            fillOpacity="0.8"
            fontSize="10"
            fontWeight="bold"
          >
            N
          </text>
          <text
            x="90"
            y="54"
            textAnchor="middle"
            fill="white"
            fillOpacity="0.5"
            fontSize="10"
            fontWeight="bold"
          >
            S
          </text>
          <text
            x="116"
            y="34"
            textAnchor="middle"
            fill="white"
            fillOpacity="0.5"
            fontSize="10"
            fontWeight="bold"
          >
            E
          </text>
          <text
            x="64"
            y="34"
            textAnchor="middle"
            fill="white"
            fillOpacity="0.5"
            fontSize="10"
            fontWeight="bold"
          >
            W
          </text>

          <g transform={`rotate(${direction}, 90, 30)`}>
            <polygon points="90,8 86,22 94,22" fill={teamColor} />
            <polygon points="90,52 86,38 94,38" fill="white" fillOpacity="0.5" />
            <circle cx="90" cy="30" r="4" fill="white" />
          </g>

          <text x="90" y="34" textAnchor="middle" fill="white" fontSize="8" fontWeight="bold">
            {Math.round(direction)}°
          </text>
        </svg>
      </div>

      <div
        className="transition-transform duration-150 ease-out origin-bottom"
        style={{ transform: `rotate(${-rodBend}deg)` }}
      >
        <svg
          width="160"
          height="220"
          viewBox="0 0 160 220"
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
            d="M80,200 Q80,150 80,100 Q80,60 85,30 Q87,20 90,15"
            stroke="url(#rod-gradient)"
            strokeWidth="6"
            strokeLinecap="round"
            fill="none"
          />
          <path
            d="M80,200 Q80,150 80,100 Q80,60 85,30 Q87,20 90,15"
            stroke="white"
            strokeOpacity="0.2"
            strokeWidth="2"
            strokeLinecap="round"
            fill="none"
          />

          <circle cx="90" cy="15" r="3" fill="#FFD700" />

          <path
            d="M90,15 Q100,25 110,50 Q120,80 115,110"
            stroke="white"
            strokeOpacity="0.6"
            strokeWidth="1"
            strokeDasharray="4,4"
            fill="none"
          />

          <circle cx="115" cy="115" r="6" fill="#FF6B35" stroke="white" strokeWidth="2" />
          <circle cx="115" cy="115" r="3" fill="white" fillOpacity="0.5" />

          <rect x="72" y="170" width="16" height="40" rx="4" fill="url(#handle-gradient)" />
          <rect x="70" y="165" width="20" height="8" rx="2" fill="#333" />
          <rect x="70" y="208" width="20" height="8" rx="2" fill="#333" />

          <circle cx="80" cy="155" r="12" fill="#555" stroke="#333" strokeWidth="2" />
          <circle cx="80" cy="155" r="6" fill="#777" />
          <circle cx="80" cy="155" r="2" fill="#333" />
        </svg>
      </div>

      {castForce > 0.1 && (
        <div className="absolute top-16 right-4 flex flex-col items-center">
          <div
            className="text-3xl font-black"
            style={{ color: "white", textShadow: "2px 2px 4px rgba(0,0,0,0.3)" }}
          >
            {Math.round(castForce * 100)}
          </div>
          <div className="text-white/80 text-xs font-bold">CAST</div>
        </div>
      )}
    </div>
  );
}
