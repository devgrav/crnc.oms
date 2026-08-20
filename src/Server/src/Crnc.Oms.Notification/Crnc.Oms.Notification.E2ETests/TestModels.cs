using System.Text.Json;

namespace Crnc.Oms.Notification.E2ETests;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Константы стенда. Пользователей Notification не сидит и не хранит — их отдаёт Security,
/// которого в наборе нет: вместо него WireMock. Поэтому «сид» здесь это не данные в БД,
/// а то, чем набор наполняет заглушку.
/// </summary>
internal static class SeedData
{
    public static readonly Guid ReceiverUserId = Guid.Parse("b6ba35b2-adff-43a6-9cd7-b408240a6d6f");
    public const string ReceiverLogin = "shon_bean";
    public const string ReceiverFirstName = "Shon";
    public const string ReceiverLastName = "Bean";
    public const string ReceiverEmail = "shon_bean@crnc.com";

    public static readonly Guid OtherUserId = Guid.Parse("35677153-5cb5-422b-a06b-328fc24caf8d");
    public const string OtherEmail = "helen_smith@crnc.com";

    // Ключ навязывается контейнерам через Auth:JwtBase64SymmetricKey, поэтому набор
    // не зависит ни от Security, ни от ротации ключей в appsettings.
    public const string JwtBase64SymmetricKey = "Y3JuYy1vbXMtbm90aWYtZTJlLXRlc3RzLWtleS0wMDE=";
    public const string JwtIssuer = "OmsCrncAuthServer";
    public const string JwtAudience = "OmsCrncApis";

    /// <summary>Отправитель, который Gateway подставляет сам (см. SendEmailInputDto).</summary>
    public const string GatewaySenderEmail = "notifications@crnc.ru";
}

/// <summary>Имена очередей и exchange'ей. Send в MassTransit идёт в exchange, одноимённый очереди.</summary>
internal static class BusNames
{
    public const string SendNotificationToUser = "sendNotificationToUser";
    public const string SendEmailNotificationToReceiver = "sendEmailNotificationToReceiver";
    public const string SendPushNotificationToReceiver = "sendPushNotificationToReceiver";

    public const string EmailSpyQueue = "e2e-spy-send-email";
    public const string PushSpyQueue = "e2e-spy-send-push";

    public const string SendNotificationToUserType =
        "urn:message:Crnc.Oms.Messaging.Contract.Commands:SendNotificationToUserCommand";
    public const string SendEmailNotificationToReceiverType =
        "urn:message:Crnc.Oms.Messaging.Contract.Commands:SendEmailNotificationToReceiverCommand";
    public const string SendPushNotificationToReceiverType =
        "urn:message:Crnc.Oms.Messaging.Contract.Commands:SendPushNotificationToReceiverCommand";
}

// --- запросы/ответы HTTP, переобъявлены локально (ссылок на код сервисов у набора нет) ---

public sealed record SendToUserRequest(Guid UserId, string? Message);

public sealed record SendNotificationResponse(Guid MessageId);

public sealed record SendEmailRequest(
    Guid? MessageId,
    string? SenderEmail,
    string? ReceiverEmail,
    string? Message);

public sealed record SendPushRequest(
    Guid MessageId,
    Guid ReceiverUserId,
    string? Message);

/// <summary>Форма ответа Security на GET /api/users/{id} — её ждёт UserInfoGateway.</summary>
public sealed record UserInfoStub(Guid Id, Guid UserId, string Login, string Email)
{
    public static UserInfoStub Receiver() => new(
        SeedData.ReceiverUserId, SeedData.ReceiverUserId, SeedData.ReceiverLogin, SeedData.ReceiverEmail);

    public static UserInfoStub WithoutEmail() => new(
        SeedData.ReceiverUserId, SeedData.ReceiverUserId, SeedData.ReceiverLogin, string.Empty);
}
