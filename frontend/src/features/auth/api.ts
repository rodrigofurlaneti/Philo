import { api } from "../../lib/apiClient";
import type { SessionResponse } from "../../lib/types";

export const createVisitorSession = (organizationId: number, displayName: string): Promise<SessionResponse> =>
  api<SessionResponse>("/api/auth/visitor-sessions", {
    method: "POST",
    body: JSON.stringify({ organizationId, displayName }),
  });
