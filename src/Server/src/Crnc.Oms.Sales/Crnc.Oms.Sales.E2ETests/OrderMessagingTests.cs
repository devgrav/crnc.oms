using System.Net;
using FluentAssertions;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Периметр этих тестов — только факт отправки: сообщение легло в очередь.
/// Что с ним дальше делают Production и Notification, здесь не проверяется
/// (их контейнеры не поднимаются) — см. раздел «Пререквизит» плана миграции.
/// </summary>
[Collection(SalesApiCollection.Name)]
public sealed class OrderMessagingTests
{
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(20);

    private readonly SalesApiFixture _fixture;

    public OrderMessagingTests(SalesApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EditOrder_StatusChanged_SendsNotificationCommandToQueue()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync(UserItemStub.MainManager());

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        // Счётчик меряем дельтой: фикстура общая, соседние тесты тоже пишут в эту очередь.
        var before = await _fixture.RabbitMq.GetMessageCountAsync(SalesApiFixture.SendNotificationSpyQueue);

        //Act
        var response = await OrderScenarios.EditOrderAsync(
            client, OrderScenarios.EditRequest(orderId, OrderStatuses.NeedSignoff));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await _fixture.RabbitMq.WaitForMessagesAsync(
            SalesApiFixture.SendNotificationSpyQueue, before, PublishTimeout);

        after.Should().BeGreaterThan(before,
            "смена статуса заказа отправляет команду уведомления главному менеджеру");
    }

    [Fact]
    public async Task EditOrder_ConvertedToJob_PublishesOrderConvertedToJobEvent()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync(UserItemStub.MainManager());

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        var before = await _fixture.RabbitMq.GetMessageCountAsync(SalesApiFixture.OrderConvertedToJobSpyQueue);

        //Act
        var response = await OrderScenarios.EditOrderAsync(
            client,
            OrderScenarios.EditRequest(
                orderId, OrderStatuses.ConvertedToJob, materialSource: MaterialSources.ToBeOrdered));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await _fixture.RabbitMq.WaitForMessagesAsync(
            SalesApiFixture.OrderConvertedToJobSpyQueue, before, PublishTimeout);

        after.Should().BeGreaterThan(before,
            "конвертация заказа публикует событие, на которое подписан Production");

        var order = await OrderScenarios.GetOrderAsync(client, orderId);
        order.Status.Should().Be(OrderStatuses.ConvertedToJob);
        order.JobId.Should().BeNull(
            "идентификатор работы проставляет ответное событие от Production, которого здесь нет");
    }
}
