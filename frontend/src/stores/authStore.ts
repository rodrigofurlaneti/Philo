import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { Role } from "../lib/types";

type AuthState = {
  userId: number | null;
  organizationId: number | null;
  role: Role | null;
  accessToken: string | null;
  expiresAtUtc: string | null;
  displayName: string | null;
};

type AuthActions = {
  setSession: (session: AuthState) => void;
  clear: () => void;
  isValid: () => boolean;
};

const emptyState: AuthState = {
  userId: null,
  organizationId: null,
  role: null,
  accessToken: null,
  expiresAtUtc: null,
  displayName: null,
};

export const useAuthStore = create<AuthState & AuthActions>()(
  persist(
    (set, get) => ({
      ...emptyState,
      setSession: (session) => set(session),
      clear: () => set(emptyState),
      isValid: () => {
        const { accessToken, expiresAtUtc } = get();
        if (!accessToken || !expiresAtUtc) return false;
        return new Date(expiresAtUtc).getTime() > Date.now();
      },
    }),
    { name: "Philo-auth" },
  ),
);
