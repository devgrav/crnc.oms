import type { Page } from "@playwright/test";

// data-testid у Mantine садится прямо на <input>/<textarea>, но помощник оставлен
// терпимым к обёрткам: так он переживает и точечные замены контролов.
export async function fillField(page: Page, testId: string, value: string): Promise<void> {
    const root = page.getByTestId(testId);
    const inner = root.locator("input, textarea");
    const target = (await inner.count()) > 0 ? inner.first() : root;
    await target.fill(value);
}

// Выпадающий список Mantine рендерит меню в портале, вне корня самого контрола,
// поэтому опция ищется на уровне страницы, а не внутри элемента с testid.
export async function selectOption(page: Page, testId: string, optionText: string): Promise<void> {
    await page.getByTestId(testId).click();
    await page.getByRole("option", { name: optionText, exact: true }).click();
}
