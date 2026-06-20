"use client";

import type { Team } from "@/lib/types";

interface OceanBackgroundProps {
  team?: Team;
}

export function OceanBackground({ team }: OceanBackgroundProps) {
  const gradientFrom = team === "B" ? "from-orange-400" : "from-sky-400";
  const gradientTo = team === "B" ? "to-orange-600" : "to-blue-600";

  return (
    <div className="fixed inset-0 overflow-hidden pointer-events-none">
      <div className={`absolute inset-0 bg-gradient-to-b ${gradientFrom} ${gradientTo}`} />

      <svg
        className="absolute bottom-0 left-0 w-[200%] h-32 opacity-30 animate-wave-bg"
        viewBox="0 0 1440 120"
        preserveAspectRatio="none"
        role="img"
        aria-hidden="true"
      >
        <path
          d="M0,60 C240,100 480,20 720,60 C960,100 1200,20 1440,60 L1440,120 L0,120 Z"
          fill="white"
        />
      </svg>

      <svg
        className="absolute bottom-0 left-0 w-[200%] h-24 opacity-20 animate-wave-bg"
        style={{ animationDelay: "-2s", animationDuration: "6s" }}
        viewBox="0 0 1440 100"
        preserveAspectRatio="none"
        role="img"
        aria-hidden="true"
      >
        <path
          d="M0,50 C200,80 400,20 600,50 C800,80 1000,20 1200,50 C1300,65 1380,35 1440,50 L1440,100 L0,100 Z"
          fill="white"
        />
      </svg>

      <div className="absolute top-8 left-8 w-16 h-16 bg-white/20 rounded-full blur-xl animate-float" />
      <div
        className="absolute top-20 right-12 w-12 h-12 bg-white/15 rounded-full blur-lg animate-float"
        style={{ animationDelay: "-1s" }}
      />
      <div
        className="absolute top-40 left-1/3 w-10 h-10 bg-white/10 rounded-full blur-md animate-float"
        style={{ animationDelay: "-2s" }}
      />
    </div>
  );
}
