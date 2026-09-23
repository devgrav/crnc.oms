import type { Page } from "@playwright/test";

export async function fillField(page: Page, testId: string, value: string): Promise<void> {
    const root = page.getByTestId(testId);
    const inner = root.locator("input, textarea");
    const target = (await inner.count()) > 0 ? inner.first() : root;
    await target.fill(value);
}

// Mantine рендерит меню в портале, поэтому опция ищется на уровне страницы.
export async function selectOption(page: Page, testId: string, optionText: string): Promise<void> {
    await page.getByTestId(testId).click();
    await page.getByRole("option", { name: optionText, exact: true }).click();
}
