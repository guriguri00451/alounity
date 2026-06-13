import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  allowedDevOrigins: [
    "https://localhost:3000",
    "https://127.0.0.1:3000",
    ...(process.env.LOCAL_IP ? [`https://${process.env.LOCAL_IP}:3000`] : []),
  ],
};

export default nextConfig;
