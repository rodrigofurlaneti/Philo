import { expect, test } from "@playwright/test";
import { withVisitorSession } from "../../support/session";

test.describe("Conversas", () => {
  test("mostra a casca de mensageiro com a barra de navegação e a lista", async ({ page }) => {
    await withVisitorSession(page);
    await page.goto("/conversas");

    await expect(page.getByTestId("nav-conversations")).toBeVisible();
    await expect(page.getByTestId("open-new-conversation")).toBeVisible();
    await expect(page.getByTestId("theme-toggle")).toBeVisible();
  });

  test("abre o modal de nova conversa", async ({ page }) => {
    await withVisitorSession(page);
    await page.goto("/conversas");

    await page.getByTestId("open-new-conversation").click();
    await expect(page.getByRole("dialog", { name: "Nova conversa" })).toBeVisible();
    await expect(page.getByTestId("new-conversation-submit")).toBeVisible();

    await page.getByTestId("modal-close").click();
    await expect(page.getByRole("dialog")).toBeHidden();
  });

  test("sem sessão válida, redireciona para /login", async ({ page }) => {
    await page.goto("/conversas");
    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByTestId("start-conversation")).toBeVisible();
  });

  test("alterna entre entrada de visitante e de equipe no login", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("switch-staff-mode").click();
    await expect(page.getByTestId("staff-token-input")).toBeVisible();

    await page.getByTestId("switch-visitor-mode").click();
    await expect(page.getByTestId("visitor-name")).toBeVisible();
  });
});
