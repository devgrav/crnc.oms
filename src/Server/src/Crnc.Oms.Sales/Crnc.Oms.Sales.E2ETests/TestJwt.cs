using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Sales токены не выдаёт, только проверяет — их выпускает Security, которого в этом
/// наборе нет. Поэтому токен подписываем сами тем же ключом, что задан контейнеру.
/// </summary>
internal static class TestJwt
{
    /// <summary>
    /// Клеймы пишутся короткими JWT-именами (nameid/unique_name/given_name/family_name/email/role) —
    /// ровно так их кладёт на провод JwtSecurityTokenHandler в Security. На стороне Sales
    /// JwtBearer разворачивает их обратно в длинные ClaimTypes-URI, которые и читает
    /// CurrentUserContext. Пишем короткие, чтобы проходить тот же путь, что и настоящий токен.
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

    /// <summary>Токен менеджера Shon Bean — он же владелец сид-заказа.</summary>
    public static string ForSeededManager() => Create(
        SeedData.ShonBeanUserId,
        SeedData.ShonBeanLogin,
        SeedData.ShonBeanFirstName,
        SeedData.ShonBeanLastName,
        SeedData.ShonBeanEmail);
}
