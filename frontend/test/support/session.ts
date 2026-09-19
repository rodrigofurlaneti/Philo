import type { Page } from "@playwright/test";

/**
 * Injeta uma sessão de visitante no localStorage antes de cada navegação, como o
 * authStore espera encontrar (AGENTS.md §8). ⚠️ addInitScript re-injeta a cada
 * navegação — um teste de logout/expiração precisa passar pela UI, não por aqui.
 */
export async function withVisitorSession(page: Page, overrides: { organizationId?: number } = {}) {
  const organizationId = overrides.organizationId ?? 1;
  const userId = Math.floor(Math.random() * 1_000_000) + 1;
  const expiresAtUtc = new Date(Date.now() + 3_600_000).toISOString();

  await page.addInitScript(
    ({ organizationId, userId, expiresAtUtc }) => {
      window.localStorage.setItem(
        "Philo-auth",
        JSON.stringify({
          state: {
            userId,
            organizationId,
            role: "Customer",
            accessToken: `e2e.${userId}.token`,
            expiresAtUtc,
            displayName: `Visitante ${userId}`,
          },
          version: 0,
        }),
      );
    },
    { organizationId, userId, expiresAtUtc },
  );

  return { userId, organizationId };
}
