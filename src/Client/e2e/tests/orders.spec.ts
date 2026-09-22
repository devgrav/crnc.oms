import { expect, test, type Page } from "@playwright/test";
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

    test("ConvertOrderToJob_SignedOrder_ReachesProduction", async ({ page }) => {
        //Arrange - конверсия идёт цепочкой статусов, разрешённой доменом Sales:
        //Not sent -> Need signoff -> Signed -> Converted to job.
        const order = await createOrder(page);
        await setStatus(page, order.jobDescription, "Need signoff", true);
        await setStatus(page, order.jobDescription, "Signed");

        //Act - карточка не должна блокироваться в момент выбора статуса,
        //иначе сохранить перевод нечем: кнопка Save исчезает вместе с формой.
        await setStatus(page, order.jobDescription, "Converted to job");

        //Assert
        await expect(orderRow(page, order.jobDescription).getByTestId("order-status"))
            .toHaveText("Converted to job");

        //Job создаётся Production'ом асинхронно, по событию из шины, а сетка jobs
        //сама не перезапрашивается - поэтому опрашиваем её перезагрузкой страницы.
        await page.getByTestId("nav-jobs").click();
        await expect(page.getByTestId("jobs-grid")).toBeVisible();
        await expect.poll(
            async () => {
                await page.reload();
                return page.getByTestId("job-row").filter({ hasText: order.jobDescription }).count();
            },
            { timeout: 30_000, intervals: [1_000, 2_000, 3_000] },
        ).toBeGreaterThan(0);
    });
});

async function setStatus(page: Page, jobDescription: string, status: string, withDetails = false) {
    await orderRow(page, jobDescription).getByTestId("order-edit").click();
    await expect(page.getByTestId("order-card")).toBeVisible();

    await selectOption(page, "order-status-select", status);

    if (withDetails) {
        await selectOption(page, "order-materialSource", "Stock");
        await selectOption(page, "order-signoffType", "Email");
    }

    await page.getByTestId("order-save").click();
    await expect(page.getByTestId("order-card")).toBeHidden();
}
