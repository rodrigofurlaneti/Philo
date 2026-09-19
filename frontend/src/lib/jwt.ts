import type { Role } from "./types";

type JwtClaims = { sub: string; org: string; role: Role; exp: number };

/**
 * Decodifica (sem verificar assinatura) um token já emitido pelo backend — usado apenas para
 * a entrada manual de sessão da equipe, já que não há login de staff pelo navegador
 * (README: sessão interna é emitida pelo backend do próprio site, protegida por API key).
 */
export function decodeJwt(token: string): JwtClaims | null {
  try {
    const [, payload] = token.split(".");
    if (!payload) return null;
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(normalized)
        .split("")
        .map((c) => `%${c.charCodeAt(0).toString(16).padStart(2, "0")}`)
        .join(""),
    );
    const claims = JSON.parse(json) as Record<string, unknown>;
    if (typeof claims.sub !== "string" || typeof claims.org !== "string" || typeof claims.role !== "string" || typeof claims.exp !== "number") {
      return null;
    }
    return { sub: claims.sub, org: claims.org, role: claims.role as Role, exp: claims.exp };
  } catch {
    return null;
  }
}
