using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.Production.E2ETests;

/// <summary>
/// Production токены только валидирует - их выпускает Security, которого в этом
/// наборе нет. Поэтому токен подписываем сами тем же ключом, что задан контейнеру.
/// Перенесено из Crnc.Oms.Sales.E2ETests почти дословно.
/// </summary>
internal static class TestJwt
{
    /// <summary>
    /// Клеймы пишутся короткими JWT-именами (nameid/unique_name/given_name/family_name/email/role) -
    /// ровно так их кладёт на провод JwtSecurityTokenHandler в Security. На стороне Production
    /// JwtBearer разворачивает их обратно в длинные ClaimTypes-URI. JobsController помечен
    /// просто [Authorize] - проверок ролей в Production нет ни одной, роль в клеймах нужна
    /// только для формы токена, а не для авторизации.
    /// </summary>
    public static string Create(
        Guid userId,
        string login,
        string firstName,
        string lastName,
        string email,
        string role = "Manager")
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

    /// <summary>Токен произвольного е2е-пользователя - Production не сверяет его ни с
    /// одним конкретным сидом (в отличие от Sales, у Production нет сид-пользователей).</summary>
    public static string ForTestUser() => Create(
        Guid.NewGuid(),
        "e2e_tester",
        "E2e",
        "Tester",
        "e2e_tester@crnc.com");
}
