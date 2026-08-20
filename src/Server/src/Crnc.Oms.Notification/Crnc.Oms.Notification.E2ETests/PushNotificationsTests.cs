using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace Crnc.Oms.Notification.E2ETests;

[Collection(NotificationApiCollection.Name)]
public sealed class PushNotificationsTests
{
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(20);

    private readonly NotificationApiFixture _fixture;

    public PushNotificationsTests(NotificationApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Send_ValidBody_DeliversMessageToAddressedUserHub()
    {
        //Arrange
        // Главная проверка набора: разом покрывает DI, SignalRPushGateway,
        // IHubContext<PushHub, IPushNotificationClient>, маппинг хаба и JWT из query string.
        // Роль notification-push-client играет клиент внутри тестового процесса.
        await using var receiver = await ConnectAsync(TestJwt.ForReceiver());
        var received = CaptureMessages(receiver);

        var request = new SendPushRequest(Guid.NewGuid(), SeedData.ReceiverUserId, "push http probe");

        //Act
        var response = await _fixture.PushClient.PostAsJsonAsync(
            "/api/pushNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await WaitForMessageAsync(received, "push http probe", DeliveryTimeout);
        message.Should().NotBeNull();
        message!.Value.UserId.Should().Be(SeedData.ReceiverUserId.ToString());
    }

    [Fact]
    public async Task SendPushNotificationToReceiverCommand_FromBus_DeliversMessageToHub()
    {
        //Arrange
        await using var receiver = await ConnectAsync(TestJwt.ForReceiver());
        var received = CaptureMessages(receiver);

        var marker = $"push bus probe {Guid.NewGuid():N}";

        //Act
        await _fixture.RabbitMq.PublishCommandAsync(
            BusNames.SendPushNotificationToReceiver,
            BusNames.SendPushNotificationToReceiverType,
            new
            {
                messageId = Guid.NewGuid(),
                receiverUserId = SeedData.ReceiverUserId,
                message = marker
            });

        //Assert
        var message = await WaitForMessageAsync(received, marker, DeliveryTimeout);
        message.Should().NotBeNull("команда из шины обязана доехать до хаба");
    }

    [Fact]
    public async Task Send_ToOtherUser_IsNotDeliveredToUnaddressedConnection()
    {
        //Arrange
        // Адресация Clients.User(...) строится на клейме nameid из токена.
        await using var receiver = await ConnectAsync(TestJwt.ForReceiver());
        await using var other = await ConnectAsync(TestJwt.ForOtherUser());

        var receiverMessages = CaptureMessages(receiver);
        var otherMessages = CaptureMessages(other);

        var marker = $"addressing probe {Guid.NewGuid():N}";
        var request = new SendPushRequest(Guid.NewGuid(), SeedData.OtherUserId, marker);

        //Act
        var response = await _fixture.PushClient.PostAsJsonAsync(
            "/api/pushNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var delivered = await WaitForMessageAsync(otherMessages, marker, DeliveryTimeout);
        delivered.Should().NotBeNull("сообщение адресовано именно этому пользователю");

        receiverMessages.Should().NotContain(m => m.Message == marker,
            "чужое уведомление не должно приходить в соединение другого пользователя");
    }

    [Fact]
    public async Task Send_MissingMessage_ReturnsBadRequestWithCamelCaseModelStateKeys()
    {
        //Arrange
        var request = new SendPushRequest(Guid.NewGuid(), SeedData.ReceiverUserId, null);

        //Act
        var response = await _fixture.PushClient.PostAsJsonAsync(
            "/api/pushNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var raw = await response.Content.ReadAsStringAsync();
        raw.Should().Contain("\"message\"");
        raw.Should().NotContain("\"Message\"");
    }

    private async Task<HubConnection> ConnectAsync(string jwt)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(_fixture.PushHubUrl, options => options.AccessTokenProvider = () => Task.FromResult(jwt)!)
            .Build();

        await connection.StartAsync();

        return connection;
    }

    private static List<PushMessage> CaptureMessages(HubConnection connection)
    {
        var received = new List<PushMessage>();

        connection.On<string, string>("ReceivePushMessageAsync", (userId, message) =>
        {
            lock (received)
                received.Add(new PushMessage(userId, message));
        });

        return received;
    }

    private static async Task<PushMessage?> WaitForMessageAsync(
        List<PushMessage> received, string marker, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            lock (received)
            {
                var match = received.FirstOrDefault(m => m.Message == marker);
                if (match.Message is not null)
                    return match;
            }

            await Task.Delay(200);
        }

        return null;
    }

    private readonly record struct PushMessage(string UserId, string Message);
}
