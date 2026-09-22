import { expect, type Page } from "@playwright/test";
import { fillField } from "./form";
import type { SeedUser } from "./seed";

// Логин делается через UI на каждый тест намеренно: текущий SPA хранит пользователя
// в sessionStorage, а Playwright `storageState` сохраняет только cookies и localStorage,
// поэтому переиспользовать состояние между контекстами нечем.
export async function loginAs(page: Page, user: SeedUser): Promise<void> {
    await page.goto("/login");
    await fillField(page, "login-login", user.login);
    await fillField(page, "login-password", user.password);
    await page.getByTestId("login-submit").click();

    // Логин считается состоявшимся, когда ушли с /login: менеджера кидает на список
    // заказов, админа — туда же, но с доступом к /users.
    await expect(page).not.toHaveURL(/\/login$/);
}

export async function logout(page: Page): Promise<void> {
    await page.getByTestId("user-signout").click();
    await expect(page).toHaveURL(/\/login$/);
}
