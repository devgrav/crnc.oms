using System.Net;
using FluentAssertions;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Проверяет исходящий вызов Sales → Security (EmployeeSecurityGateway) против заглушки.
/// Под §7 плана миграции: гейтвей переезжает с RestSharp на HttpClient, и склейка
/// BaseAddress с относительным путём — ровно то место, где это ломается молча.
/// </summary>
[Collection(SalesApiCollection.Name)]
public sealed class SecurityIntegrationTests
{
    private readonly SalesApiFixture _fixture;

    public SecurityIntegrationTests(SalesApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EditOrder_StatusChanged_RequestsMainManagersFromSecurity()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync(UserItemStub.MainManager());

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        //Act
        var response = await OrderScenarios.EditOrderAsync(
            client, OrderScenarios.EditRequest(orderId, OrderStatuses.NeedSignoff));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var requests = await _fixture.SecurityStub.GetRecordedRequestsAsync();
        var usersRequest = requests.Should()
            .ContainSingle(x => x.Method == "GET" && x.Url.StartsWith("/api/users"))
            .Subject;

        usersRequest.Url.Should().Contain("roles=", "гейтвей запрашивает пользователей по роли");
        usersRequest.Headers.Should().ContainKey("Authorization");
        usersRequest.Headers["Authorization"].Should().StartWith("Bearer ",
            "гейтвей пробрасывает токен текущего пользователя дальше в Security");
    }

    [Fact]
    public async Task EditOrder_SecurityUnavailable_StillSucceeds()
    {
        //Arrange - OrderStatusChangedHandler ловит ошибку гейтвея и только пишет в лог,
        //поэтому недоступность Security не должна валить основной сценарий.
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUsersFailureAsync(500);

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        //Act
        var response = await OrderScenarios.EditOrderAsync(
            client, OrderScenarios.EditRequest(orderId, OrderStatuses.NeedSignoff));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await OrderScenarios.GetOrderAsync(client, orderId);
        order.Status.Should().Be(OrderStatuses.NeedSignoff, "статус заказа сохраняется независимо от уведомлений");
    }
}
