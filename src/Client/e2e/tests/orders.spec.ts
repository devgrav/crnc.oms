import { expect, test } from "@playwright/test";
import { loginAs } from "../support/auth";
import { selectOption } from "../support/form";
import { createOrder, orderRow } from "../support/orders";
import { SeedUsers } from "../support/seed";

test.describe("Orders", () => {
    test.beforeEach(async ({ page }) => {
        await loginAs(page, SeedUsers.manager);
        await page.goto("/orders");
    });

    test("OrdersGrid_AuthenticatedManager_ShowsSeededOrders", async ({ page }) => {
        //Arrange, Act - сетка грузится сама при монтировании

        //Assert
        await expect(page.getByTestId("orders-grid")).toBeVisible();
        expect(await page.getByTestId("order-row").count()).toBeGreaterThan(0);
    });

    test("CreateOrder_ValidData_AppearsInGrid", async ({ page }) => {
        //Arrange, Act
        const order = await createOrder(page);

        //Assert
        await expect(orderRow(page, order.jobDescription)).toBeVisible();
    });

    test("CreateOrder_EmptyRequiredFields_ShowsValidationSummary", async ({ page }) => {
        //Arrange - регрессия на формат ошибок валидации: Sales отдаёт
        //ValidationProblemDetails с обёрткой errors, и SPA разбирает именно её.
        //См. §4.3 плана миграции - нормализатор должен пережить переезд.
        await page.getByTestId("orders-add").click();
        await expect(page.getByTestId("order-card")).toBeVisible();

        //Act
        await page.getByTestId("order-save").click();

        //Assert
        await expect(page.getByTestId("order-validation-summary")).toBeVisible();
        await expect(page.getByTestId("order-card")).toBeVisible();
    });

    test("EditOrder_ChangedStatus_PersistsToGrid", async ({ page }) => {
        //Arrange - правим свежесозданный заказ, а не первый попавшийся: у заказа
        //в статусе Closed/ConvertedToJob кнопки Save нет вовсе.
        const order = await createOrder(page);

        //Act
        await orderRow(page, order.jobDescription).getByTestId("order-edit").click();
        await expect(page.getByTestId("order-card")).toBeVisible();

        await selectOption(page, "order-status-select", "Need signoff");
        await selectOption(page, "order-materialSource", "Stock");
        await selectOption(page, "order-signoffType", "Email");
        await page.getByTestId("order-save").click();

        //Assert
        await expect(page.getByTestId("order-card")).toBeHidden();
        await expect(orderRow(page, order.jobDescription).getByTestId("order-status"))
            .toHaveText("Need signoff");
    });
});
