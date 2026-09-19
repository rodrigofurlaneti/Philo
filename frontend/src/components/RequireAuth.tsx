import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";

/**
 * Checa accessToken e validade por expiresAtUtc; sem sessão, redireciona para /login
 * guardando a rota de origem em location.state.from (AGENTS.md §6).
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const isValid = useAuthStore((s) => s.isValid());
  const location = useLocation();

  if (!isValid) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  return children;
}
