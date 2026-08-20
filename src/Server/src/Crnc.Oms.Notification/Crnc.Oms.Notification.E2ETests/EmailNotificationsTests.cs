using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Notification.E2ETests;

[Collection(NotificationApiCollection.Name)]
public sealed class EmailNotificationsTests
{
    private static readonly TimeSpan BusTimeout = TimeSpan.FromSeconds(20);

    private readonly NotificationApiFixture _fixture;

    public EmailNotificationsTests(NotificationApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Send_ValidBody_ReturnsOkWithoutToken()
    {
        //Arrange
        // Baseline, измеренный в фазе 0: у EmailNotificationsController нет ни [Authorize],
        // ни [AllowAnonymous], а в Startup.cs Email вызывается UseAuthentication() без
        // AddAuthentication — и на netcoreapp3.1 это безвредно, эндпойнт отвечает 200.
        // При переходе на minimal hosting схема аутентификации появится (§3 плана);
        // этот тест сторожит, чтобы эндпойнт от этого не стал 401.
        var request = new SendEmailRequest(
            Guid.NewGuid(), SeedData.GatewaySenderEmail, SeedData.ReceiverEmail, "email http probe");

        //Act
        var response = await _fixture.EmailClient.PostAsJsonAsync(
            "/api/emailNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Send_MissingAddresses_ReturnsBadRequestWithCamelCaseModelStateKeys()
    {
        //Arrange
        // Стережёт §4 плана миграции. Форма ответа снята с живого сервиса в фазе 0:
        // {"senderEmail":["The SenderEmail field is required."],"receiverEmail":[...]}
        var request = new SendEmailRequest(null, null, null, "no addresses");

        //Act
        var response = await _fixture.EmailClient.PostAsJsonAsync(
            "/api/emailNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var raw = await response.Content.ReadAsStringAsync();
        raw.Should().Contain("\"senderEmail\"");
        raw.Should().Contain("\"receiverEmail\"");
        raw.Should().NotContain("\"SenderEmail\"");
        raw.Should().NotContain("\"ReceiverEmail\"");
    }

    [Fact]
    public async Task Send_InvalidEmailFormat_ReturnsBadRequest()
    {
        //Arrange
        var request = new SendEmailRequest(
            Guid.NewGuid(), "not-an-email", SeedData.ReceiverEmail, "bad sender");

        //Act
        var response = await _fixture.EmailClient.PostAsJsonAsync(
            "/api/emailNotifications", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendEmailNotificationToReceiverCommand_FromBus_KeepsAddresseesFromCommand()
    {
        //Arrange
        // Раньше здесь фиксировался дефект: SendEmailNotificationToReceiverConsumer копировал
        // в SendEmailMessageInputDto только Message, теряя SenderEmail/ReceiverEmail, и письмо
        // «уходило» никому. Дефект измерен в фазе 0 сравнением двух строк лога и починен в
        // фазе 5 — тест переписан на правильное поведение тем же коммитом (§9, группа B).
        var marker = $"bus email probe {Guid.NewGuid():N}";

        //Act
        await _fixture.RabbitMq.PublishCommandAsync(
            BusNames.SendEmailNotificationToReceiver,
            BusNames.SendEmailNotificationToReceiverType,
            new
            {
                messageId = Guid.NewGuid(),
                senderEmail = SeedData.GatewaySenderEmail,
                receiverEmail = SeedData.ReceiverEmail,
                message = marker
            });

        //Assert
        var logLine = await WaitForLogLineAsync(marker, BusTimeout);

        logLine.Should().NotBeNull("консьюмер обязан съесть команду и написать строку в лог");
        logLine.Should().Contain($"sender : {SeedData.GatewaySenderEmail} to receiver {SeedData.ReceiverEmail}",
            "адресаты обязаны доехать из команды до сервиса");
    }

    private async Task<string?> WaitForLogLineAsync(string marker, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            var logs = await _fixture.GetEmailServiceLogsAsync();

            var line = logs
                .Split('\n')
                .FirstOrDefault(l => l.Contains(marker, StringComparison.Ordinal));

            if (line is not null)
                return line;

            await Task.Delay(500);
        }

        return null;
    }
}
