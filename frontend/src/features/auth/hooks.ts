import { useQuery } from "@tanstack/react-query";
import * as api from "./api";

/**
 * Chaves fora do padrão userId-first das outras features de propósito: estas consultas
 * rodam ANTES de existir sessão (tela de login), então não há usuário para escopar o cache.
 */
export const devLoginKeys = {
  organizations: ["dev-login", "organizations"] as const,
  members: (organizationId: number) => ["dev-login", "organizations", organizationId, "members"] as const,
};

export function useDevOrganizations(enabled: boolean) {
  return useQuery({
    queryKey: devLoginKeys.organizations,
    queryFn: api.listDevOrganizations,
    enabled,
    staleTime: 60_000,
    retry: false,
  });
}

export function useDevOrganizationMembers(organizationId: number | null, enabled: boolean) {
  return useQuery({
    queryKey: devLoginKeys.members(organizationId ?? 0),
    queryFn: () => api.listDevOrganizationMembers(organizationId!),
    enabled: enabled && organizationId !== null,
    staleTime: 30_000,
    retry: false,
  });
}
