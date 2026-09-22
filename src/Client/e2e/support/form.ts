import type { Page } from "@playwright/test";

// Semantic UI раскладывает data-testid по-разному в зависимости от контрола:
// Form.Input вешает его на обёртку <div class="ui input">, а Form.TextArea - прямо
// на <textarea>. Хелпер скрывает это различие, чтобы тесты не зависели от верстки
// кита, который всё равно уедет при миграции.
export async function fillField(page: Page, testId: string, value: string): Promise<void> {
    const root = page.getByTestId(testId);
    const inner = root.locator("input, textarea");
    const target = (await inner.count()) > 0 ? inner.first() : root;
    await target.fill(value);
}

// Semantic UI Dropdown: клик по корню раскрывает меню, опции рендерятся как
// role="option" внутри того же корня.
export async function selectOption(page: Page, testId: string, optionText: string): Promise<void> {
    const dropdown = page.getByTestId(testId);
    await dropdown.click();
    await dropdown.getByRole("option", { name: optionText, exact: true }).click();
}
