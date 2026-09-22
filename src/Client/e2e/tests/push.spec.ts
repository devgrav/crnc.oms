import { expect, test } from "@playwright/test";
import { loginAs } from "../support/auth";
import { selectOption } from "../support/form";
import { createOrder, orderRow } from "../support/orders";
import { SeedUsers } from "../support/seed";

// Самый хрупкий сценарий набора и одновременно самый ценный: сегодня это
// единственное доказательство, что цепочка Sales -> шина -> Gateway -> Push ->
// SignalR -> колокольчик вообще жива. См. §0 и §8 плана миграции.
test.describe("Push notifications", () => {
    test("OrderStatusChanged_MainManagerConnected_ShowsBellBadge", async ({ browser }) => {
        test.slow();

        //Arrange - получатель уведомления держит открытую сессию: SignalR-подключение
        //поднимается в Notifications при монтировании, адресация идёт по claim nameid.
        const receiverContext = await browser.newContext();
        const receiver = await receiverContext.newPage();
        await loginAs(receiver, SeedUsers.mainManager);
        await receiver.goto("/orders");
        await expect(receiver.getByTestId("notifications-bell")).toBeVisible();
        await expect(receiver.getByTestId("notifications-count")).toHaveCount(0);

        const actorContext = await browser.newContext();
        const actor = await actorContext.newPage();
        await loginAs(actor, SeedUsers.admin);
        await actor.goto("/orders");

        //Act - статус меняет другой пользователь, в своей сессии
        const order = await createOrder(actor);
        await orderRow(actor, order.jobDescription).getByTestId("order-edit").click();
        await expect(actor.getByTestId("order-card")).toBeVisible();
        await selectOption(actor, "order-status-select", "Need signoff");
        await selectOption(actor, "order-materialSource", "Stock");
        await selectOption(actor, "order-signoffType", "Email");
        await actor.getByTestId("order-save").click();
        await expect(actor.getByTestId("order-card")).toBeHidden();

        //Assert - доставка асинхронна относительно HTTP-ответа, ждём бейдж
        await expect(receiver.getByTestId("notifications-count")).toBeVisible({ timeout: 30_000 });

        await receiverContext.close();
        await actorContext.close();
    });
});
