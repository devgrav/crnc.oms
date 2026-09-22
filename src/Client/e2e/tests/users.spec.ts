import { expect, test, type Page } from "@playwright/test";
import { loginAs } from "../support/auth";
import { fillField, selectOption } from "../support/form";
import { SeedUsers, unique } from "../support/seed";

test.describe("Users", () => {
    test.beforeEach(async ({ page }) => {
        await loginAs(page, SeedUsers.admin);
        await page.goto("/users");
        await expect(page.getByTestId("users-cards")).toBeVisible();
    });

    test("CreateUser_ValidData_AppearsInCards", async ({ page }) => {
        //Arrange - фикстура стенда общая, логин обязан быть уникальным на тест
        const login = unique("e2e_user");

        //Act
        await createUser(page, login);

        //Assert - карточка ищется поиском, а не глазами: на странице помещается
        //8 карточек, и новый пользователь уезжает на последнюю страницу.
        await searchUser(page, login, "Manager");
        await expect(userCard(page, login)).toBeVisible();
    });

    test("CreateUser_MissingRequiredField_ShowsValidationSummary", async ({ page }) => {
        //Arrange - регрессия на второй формат ошибок: Security отдаёт плоский
        //SerializableError без обёртки errors (нет [ApiController]), и SPA
        //разбирает его отдельной веткой. См. §4.3 плана миграции.
        await page.getByTestId("users-add").click();
        await expect(page.getByTestId("user-card-edit")).toBeVisible();

        //Act
        await page.getByTestId("user-save").click();

        //Assert
        await expect(page.getByTestId("user-card-edit")).toBeVisible();
        await expect(page.getByTestId("user-validation-summary")).toBeVisible();
    });

    test("DeleteUser_Confirmed_RemovesCard", async ({ page }) => {
        //Arrange
        const login = unique("e2e_user");
        await createUser(page, login);
        await searchUser(page, login, "Manager");
        await expect(userCard(page, login)).toBeVisible();

        //Act
        await userCard(page, login).getByTestId("user-delete").click();
        await page.getByRole("button", { name: "OK" }).click();

        //Assert
        await expect(userCard(page, login)).toHaveCount(0);
    });

    test("SearchUsers_ByLoginAndRole_FiltersCards", async ({ page }) => {
        //Arrange - поиск здесь клиентский, по уже загруженному списку.
        //Роль выбирается не для полноты сценария, а вынужденно: в UserCards.handleSearch
        //условие `if (this.state.search.role)` истинно и для Guid.EMPTY, поэтому поиск
        //без выбранной роли фильтрует по roleId === EMPTY и всегда даёт пусто.
        //Баг текущего приложения, чинится при миграции — см. §6.3 плана.

        //Act
        await searchUser(page, SeedUsers.mainManager.login, SeedUsers.mainManager.role);

        //Assert
        await expect(page.getByTestId("user-card")).toHaveCount(1);
        await expect(userCard(page, SeedUsers.mainManager.login)).toBeVisible();
    });
});

async function createUser(page: Page, login: string): Promise<void> {
    await page.getByTestId("users-add").click();
    await expect(page.getByTestId("user-card-edit")).toBeVisible();

    await fillField(page, "user-login", login);
    await fillField(page, "user-password", "111111");
    await selectOption(page, "user-role", "Manager");
    await fillField(page, "user-firstName", "E2E");
    await fillField(page, "user-lastName", login);
    await fillField(page, "user-email", `${login}@crnc.com`);
    await page.getByTestId("user-save").click();

    await expect(page.getByTestId("user-card-edit")).toBeHidden();
}

async function searchUser(page: Page, login: string, role: string): Promise<void> {
    await page.getByTestId("users-search-open").click();
    await fillField(page, "user-search-login", login);
    await selectOption(page, "user-search-role", role);
    await page.getByTestId("user-search-submit").click();
}

function userCard(page: Page, login: string) {
    return page.getByTestId("user-card").filter({ hasText: login });
}
