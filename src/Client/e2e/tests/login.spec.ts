import { expect, test } from "@playwright/test";
import { loginAs, logout } from "../support/auth";
import { fillField } from "../support/form";
import { SeedUsers } from "../support/seed";

test.describe("Login", () => {
    test("SignIn_WrongPassword_ShowsError", async ({ page }) => {
        //Arrange
        await page.goto("/login");

        //Act
        await fillField(page, "login-login", SeedUsers.admin.login);
        await fillField(page, "login-password", "wrong-password");
        await page.getByTestId("login-submit").click();

        //Assert - бэкенд отдаёт на это голую строку, а не ProblemDetails
        //(Security/AccountsController.cs); см. §4.3 плана миграции.
        await expect(page.getByTestId("login-error")).toBeVisible();
        await expect(page).toHaveURL(/\/login$/);
    });

    test("SignIn_ValidCredentials_LeavesLoginPage", async ({ page }) => {
        //Arrange, Act
        await loginAs(page, SeedUsers.admin);

        //Assert
        await expect(page.getByTestId("user-login")).toHaveText(SeedUsers.admin.login);
    });

    test("SignOut_AuthenticatedUser_ReturnsToLogin", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.manager);

        //Act
        await logout(page);

        //Assert
        await expect(page.getByTestId("login-submit")).toBeVisible();
    });

    test("PrivateRoute_NotAuthenticated_RedirectsToLogin", async ({ page }) => {
        //Arrange, Act
        await page.goto("/orders");

        //Assert
        await expect(page).toHaveURL(/\/login$/);
    });
});
