import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  allowedDevOrigins: [
    "localhost",
    "127.0.0.1",
    ...(process.env.LOCAL_IP ? [process.env.LOCAL_IP] : []),
  ],
};

export default nextConfig;
