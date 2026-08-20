using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Notification.E2ETests;

[Collection(NotificationApiCollection.Name)]
public sealed class GatewayNotificationsTests
{
    private static readonly TimeSpan BusTimeout = TimeSpan.FromSeconds(20);

    /// <summary>Окно, в течение которого негативный тест ждёт «ничего не приехало».</summary>
    private static readonly TimeSpan SilenceWindow = TimeSpan.FromSeconds(3);

    private readonly NotificationApiFixture _fixture;

    public GatewayNotificationsTests(NotificationApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SendToUser_NoToken_ReturnsUnauthorized()
    {
        //Arrange
        var request = new SendToUserRequest(SeedData.ReceiverUserId, "no token");

        //Act
        var response = await _fixture.GatewayClient.PostAsJsonAsync(
            "/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendToUser_KnownUser_ReturnsOkAndSendsCommandToBothChannels()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUserAsync(SeedData.ReceiverUserId, UserInfoStub.Receiver());

        var marker = $"known user probe {Guid.NewGuid():N}";
        var client = _fixture.CreateAuthorizedGatewayClient();
        var request = new SendToUserRequest(SeedData.ReceiverUserId, marker);

        //Act
        var response = await client.PostAsJsonAsync("/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<SendNotificationResponse>(JsonDefaults.Options);
        body!.MessageId.Should().NotBeEmpty();

        var emailCommand = await _fixture.RabbitMq.WaitForMessageAsync(
            BusNames.EmailSpyQueue, marker, BusTimeout);
        var pushCommand = await _fixture.RabbitMq.WaitForMessageAsync(
            BusNames.PushSpyQueue, marker, BusTimeout);

        emailCommand.Should().NotBeNull("Gateway разводит одно уведомление по обоим каналам");
        pushCommand.Should().NotBeNull();

        // Разрешение канала доставки: email взят из карточки Security, отправитель подставлен
        // самим Gateway — в исходной команде от Sales ни того, ни другого нет.
        emailCommand.Should().Contain(SeedData.ReceiverEmail);
        emailCommand.Should().Contain(SeedData.GatewaySenderEmail);
        pushCommand.Should().Contain(SeedData.ReceiverUserId.ToString());
    }

    [Fact]
    public async Task SendToUser_KnownUser_CallsSecurityWithoutAuthorizationHeader()
    {
        //Arrange
        // Проверяется не «дырка в безопасности», а несущее решение контекста: GET /api/users/{id}
        // у Security помечен [AllowAnonymous], потому что контракт уведомления намеренно не несёт
        // параметров канала доставки и Notification добывает их сам. Если однажды здесь появится
        // токен — этот тест первым скажет, что замысел изменили.
        // См. «Разрешение канала доставки» в docs/migrations/notification-net10-migration-plan.md.
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUserAsync(SeedData.ReceiverUserId, UserInfoStub.Receiver());

        var marker = $"outbound call probe {Guid.NewGuid():N}";
        var client = _fixture.CreateAuthorizedGatewayClient();
        var request = new SendToUserRequest(SeedData.ReceiverUserId, marker);

        //Act
        var response = await client.PostAsJsonAsync("/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var recorded = await _fixture.SecurityStub.WaitForRequestsAsync(
            $"/api/users/{SeedData.ReceiverUserId}", BusTimeout);

        recorded.Should().ContainSingle();
        recorded[0].Method.Should().Be("GET");
        recorded[0].Headers.Should().NotContainKey("Authorization");

        // Забираем свои сообщения из шпионских очередей, чтобы не мешать соседним тестам.
        await _fixture.RabbitMq.WaitForMessageAsync(BusNames.EmailSpyQueue, marker, BusTimeout);
        await _fixture.RabbitMq.WaitForMessageAsync(BusNames.PushSpyQueue, marker, BusTimeout);
    }

    [Fact]
    public async Task SendToUser_UnknownUser_ReturnsBadRequestAndSendsNothing()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUserFailureAsync(SeedData.ReceiverUserId, 404);

        var marker = $"unknown user probe {Guid.NewGuid():N}";
        var client = _fixture.CreateAuthorizedGatewayClient();
        var request = new SendToUserRequest(SeedData.ReceiverUserId, marker);

        //Act
        var response = await client.PostAsJsonAsync("/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await _fixture.RabbitMq.AnyMessageAsync(BusNames.EmailSpyQueue, marker, SilenceWindow))
            .Should().BeFalse();
        (await _fixture.RabbitMq.AnyMessageAsync(BusNames.PushSpyQueue, marker, SilenceWindow))
            .Should().BeFalse();
    }

    [Fact]
    public async Task SendToUser_UserWithoutEmail_ReturnsBadRequestAndSendsNothing()
    {
        //Arrange
        // Разрешение канала доставки живёт в Gateway: нет email — нет и уведомления,
        // причём ни по одному из каналов, включая push, которому email не нужен.
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUserAsync(SeedData.ReceiverUserId, UserInfoStub.WithoutEmail());

        var marker = $"no email probe {Guid.NewGuid():N}";
        var client = _fixture.CreateAuthorizedGatewayClient();
        var request = new SendToUserRequest(SeedData.ReceiverUserId, marker);

        //Act
        var response = await client.PostAsJsonAsync("/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await _fixture.RabbitMq.AnyMessageAsync(BusNames.EmailSpyQueue, marker, SilenceWindow))
            .Should().BeFalse();
        (await _fixture.RabbitMq.AnyMessageAsync(BusNames.PushSpyQueue, marker, SilenceWindow))
            .Should().BeFalse();
    }

    [Fact]
    public async Task SendToUser_EmptyMessage_ReturnsBadRequestWithCamelCaseModelStateKeys()
    {
        //Arrange
        // Стережёт §4 плана миграции: Newtonsoft отдаёт ключи ModelState в camelCase через
        // CamelCasePropertyNamesContractResolver, System.Text.Json — только если явно задан
        // DictionaryKeyPolicy. Забыть его — молчаливая регрессия контракта ошибок.
        var client = _fixture.CreateAuthorizedGatewayClient();
        var request = new SendToUserRequest(SeedData.ReceiverUserId, null);

        //Act
        var response = await client.PostAsJsonAsync("/api/notifications/user", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var raw = await response.Content.ReadAsStringAsync();
        raw.Should().Contain("\"message\"");
        raw.Should().NotContain("\"Message\"");
    }

    [Fact]
    public async Task SendNotificationToUserCommand_FromBus_FansOutToBothChannels()
    {
        //Arrange
        // Настоящий вход контекста — команда от Sales, а не HTTP. Sales в наборе нет,
        // поэтому конверт MassTransit кладётся в очередь через management API.
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubUserAsync(SeedData.ReceiverUserId, UserInfoStub.Receiver());

        var marker = $"bus entry probe {Guid.NewGuid():N}";

        //Act
        await _fixture.RabbitMq.PublishCommandAsync(
            BusNames.SendNotificationToUser,
            BusNames.SendNotificationToUserType,
            new { userId = SeedData.ReceiverUserId, message = marker });

        //Assert
        (await _fixture.RabbitMq.WaitForMessageAsync(BusNames.EmailSpyQueue, marker, BusTimeout))
            .Should().NotBeNull();
        (await _fixture.RabbitMq.WaitForMessageAsync(BusNames.PushSpyQueue, marker, BusTimeout))
            .Should().NotBeNull();

        var queues = await _fixture.RabbitMq.GetQueueNamesAsync();
        queues.Should().NotContain(q => q.EndsWith("_error", StringComparison.OrdinalIgnoreCase),
            "консьюмер не должен падать — иначе команда уходит в error-очередь");
    }
}
