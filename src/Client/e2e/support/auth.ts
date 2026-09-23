import { expect, type Page } from "@playwright/test";
import { fillField } from "./form";
import type { SeedUser } from "./seed";

// Логин через UI на каждый тест: пользователь лежит в sessionStorage, а `storageState`
// сохраняет только cookies и localStorage - переиспользовать состояние нечем.
export async function loginAs(page: Page, user: SeedUser): Promise<void> {
    await page.goto("/login");
    await fillField(page, "login-login", user.login);
    await fillField(page, "login-password", user.password);
    await page.getByTestId("login-submit").click();

    await expect(page).not.toHaveURL(/\/login$/);
}

export async function logout(page: Page): Promise<void> {
    await page.getByTestId("user-signout").click();
    await expect(page).toHaveURL(/\/login$/);
}
