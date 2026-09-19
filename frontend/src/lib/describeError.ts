import { ApiError } from "./apiClient";

/** Códigos transversais, comuns a qualquer feature (AGENTS.md §3). */
export function describeCommonError(error: unknown): string | null {
  if (!(error instanceof ApiError)) return null;
  switch (error.code) {
    case "Network.Unreachable":
      return "Não foi possível conectar ao servidor. Verifique sua conexão.";
    case "Auth.SessionExpired":
      return "Sua sessão expirou. Entre novamente.";
    case "Validation.Failed":
      return error.message || "Alguns campos precisam de atenção.";
    default:
      return null;
  }
}

export function describeError(error: unknown): string {
  return describeCommonError(error) ?? (error instanceof ApiError ? error.message : "Não foi possível concluir. Tente de novo.");
}
