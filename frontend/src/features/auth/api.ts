import { api } from "../../lib/apiClient";
import type { OrganizationSummary, SessionResponse, StaffMember } from "../../lib/types";

export const createVisitorSession = (organizationId: number, displayName: string): Promise<SessionResponse> =>
  api<SessionResponse>("/api/auth/visitor-sessions", {
    method: "POST",
    body: JSON.stringify({ organizationId, displayName }),
  });

const devHeaders = (): HeadersInit => ({ "X-Internal-Api-Key": import.meta.env.VITE_DEV_INTERNAL_API_KEY ?? "" });

/**
 * Atalhos só para desenvolvimento local (LoginPage, "Login rápido (dev)"). Em produção esses
 * endpoints são chamados pelo backend do próprio site, nunca pelo navegador — ver .env.example.
 */
export const listDevOrganizations = (): Promise<OrganizationSummary[]> =>
  api<OrganizationSummary[]>("/api/internal/organizations", { headers: devHeaders() });

export const listDevOrganizationMembers = (organizationId: number): Promise<StaffMember[]> =>
  api<StaffMember[]>(`/api/internal/organizations/${organizationId}/members`, { headers: devHeaders() });

export const issueDevStaffSession = (organizationId: number, userId: number): Promise<SessionResponse> =>
  api<SessionResponse>("/api/internal/sessions", {
    method: "POST",
    headers: devHeaders(),
    body: JSON.stringify({ organizationId, userId }),
  });
