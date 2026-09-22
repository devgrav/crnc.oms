import { expect, test } from "@playwright/test";
import { loginAs } from "../support/auth";
import { SeedUsers } from "../support/seed";

test.describe("Jobs", () => {
    test("JobsGrid_AuthenticatedManager_IsReachableFromMenu", async ({ page }) => {
        //Arrange
        await loginAs(page, SeedUsers.manager);

        //Act
        await page.getByTestId("nav-jobs").click();

        //Assert - Production наполняется только из шины, поэтому пустая сетка
        //на чистом стенде это валидный результат; проверяем, что экран жив.
        await expect(page).toHaveURL(/\/jobs$/);
        await expect(page.getByTestId("jobs-grid")).toBeVisible();
    });
});
