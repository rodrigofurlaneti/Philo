import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Padrão de proxy do AGENTS.md: API em HTTP :5080 durante o desenvolvimento.
// Rode a WebApi no perfil HTTP ao testar pelo frontend — o perfil HTTPS causa 502 aqui.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "http://localhost:5080", changeOrigin: true },
      "/hubs": { target: "http://localhost:5080", changeOrigin: true, ws: true },
      "/health": { target: "http://localhost:5080", changeOrigin: true },
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          react: ["react", "react-dom", "react-router-dom"],
          charts: ["recharts"],
          dialogs: ["sweetalert2"],
        },
      },
    },
  },
});
