import { expect, test, type Locator } from "@playwright/test";
import { loginAs } from "../support/auth";
import { SeedUsers } from "../support/seed";

test.describe("Navigation", () => {
    test("TopMenu_OpenSection_IsHighlighted", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.admin);

        //Act
        await page.goto("/jobs");

        //Assert - проверяется видимый результат, а не имя css-класса: у активного
        //пункта подчёркивание есть, у остальных его нет.
        await expect(page.getByTestId("jobs-grid")).toBeVisible();
        expect(await underlineColor(page.getByTestId("nav-jobs"))).not.toBe("rgba(0, 0, 0, 0)");
        expect(await underlineColor(page.getByTestId("nav-orders"))).toBe("rgba(0, 0, 0, 0)");
    });

    test("TopMenu_NavigateToAnotherSection_MovesHighlight", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.admin);
        await page.goto("/jobs");

        //Act
        await page.getByTestId("nav-orders").click();

        //Assert
        await expect(page.getByTestId("orders-grid")).toBeVisible();
        expect(await underlineColor(page.getByTestId("nav-orders"))).not.toBe("rgba(0, 0, 0, 0)");
        expect(await underlineColor(page.getByTestId("nav-jobs"))).toBe("rgba(0, 0, 0, 0)");
    });
});

function underlineColor(link: Locator): Promise<string> {
    return link.evaluate((node) => getComputedStyle(node).borderBottomColor);
}
