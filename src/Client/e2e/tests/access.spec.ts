import { expect, test } from "@playwright/test";
import { loginAs } from "../support/auth";
import { SeedUsers } from "../support/seed";

test.describe("Access control", () => {
    test("UsersRoute_ManagerRole_ShowsForbidden", async ({ page }) => {
        //Arrange - гард в routes.tsx пускает Admin на любой роут, остальным
        //сверяет объявленные roles; см. §5.3 плана миграции.
        await loginAs(page, SeedUsers.manager);

        //Act
        await page.goto("/users");

        //Assert - именно Forbidden, а не 404 и не белый экран
        await expect(page.getByTestId("forbidden")).toBeVisible();
    });

    test("UsersRoute_AdminRole_ShowsUsers", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.admin);

        //Act
        await page.goto("/users");

        //Assert
        await expect(page.getByTestId("users-cards")).toBeVisible();
    });

    test("OrdersRoute_AdminRole_IsAllowedDespiteDeclaredRoles", async ({ page }) => {
        //Arrange - /orders объявляет roles [MainManager, Manager], но Admin
        //проходит везде. Поведение намеренное, переносится в новый гард как есть.
        await loginAs(page, SeedUsers.admin);

        //Act
        await page.goto("/orders");

        //Assert
        await expect(page.getByTestId("orders-grid")).toBeVisible();
    });

    test("UnknownRoute_AuthenticatedUser_ShowsNotFound", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.admin);

        //Act
        await page.goto("/nonexistent-route");

        //Assert
        await expect(page.getByTestId("not-found")).toBeVisible();
    });
});
