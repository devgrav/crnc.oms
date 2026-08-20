using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.Notification.E2ETests;

/// <summary>
/// Notification токены не выдаёт, только проверяет — их выпускает Security, которого в этом
/// наборе нет. Поэтому токен подписываем сами тем же ключом, что задан контейнерам.
/// Перенос из Crnc.Oms.Sales.E2ETests без изменений, кроме namespace.
/// </summary>
internal static class TestJwt
{
    /// <summary>
    /// Клеймы пишутся короткими JWT-именами (nameid/unique_name/given_name/family_name/email/role) —
    /// ровно так их кладёт на провод Security. JwtBearer на стороне сервиса разворачивает их
    /// обратно в длинные ClaimTypes-URI; SignalR берёт из nameid идентификатор пользователя,
    /// по которому потом адресует Clients.User(...).
    /// </summary>
    public static string Create(
        Guid userId,
        string login,
        string firstName,
        string lastName,
        string email,
        string role = "Main manager")
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(SeedData.JwtBase64SymmetricKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = SeedData.JwtIssuer,
            Audience = SeedData.JwtAudience,
            NotBefore = DateTime.UtcNow.AddMinutes(-1),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                ["nameid"] = userId.ToString(),
                ["unique_name"] = login,
                ["given_name"] = firstName,
                ["family_name"] = lastName,
                ["email"] = email,
                ["role"] = role
            }
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    /// <summary>Токен получателя уведомлений — Shon Bean.</summary>
    public static string ForReceiver() => Create(
        SeedData.ReceiverUserId,
        SeedData.ReceiverLogin,
        SeedData.ReceiverFirstName,
        SeedData.ReceiverLastName,
        SeedData.ReceiverEmail);

    /// <summary>Токен другого пользователя — чтобы проверить адресацию пушей.</summary>
    public static string ForOtherUser() => Create(
        SeedData.OtherUserId,
        "helen_smith",
        "Helen",
        "Smith",
        SeedData.OtherEmail);
}
