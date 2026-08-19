using System.Text.Json;

namespace Crnc.Oms.Sales.E2ETests;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Значения, которые <c>SalesDbInitializer</c> кладёт в базу при каждом старте сервиса,
/// и настройки аутентификации, которые фикстура передаёт контейнеру.
/// </summary>
internal static class SeedData
{
    public static readonly Guid SeededOrderId = Guid.Parse("5c5c6017-1b1f-4a46-b423-455ad4f273fe");

    public static readonly Guid ShonBeanUserId = Guid.Parse("b6ba35b2-adff-43a6-9cd7-b408240a6d6f");
    public const string ShonBeanLogin = "shon_bean";
    public const string ShonBeanFirstName = "Shon";
    public const string ShonBeanLastName = "Bean";
    public const string ShonBeanEmail = "shon_bean@crnc.com";

    // Собственный ключ подписи тестов: фикстура задаёт его контейнеру через
    // Auth:JwtBase64SymmetricKey, поэтому набор не зависит ни от значения в
    // appsettings.json, ни от его будущей ротации.
    public const string JwtBase64SymmetricKey = "Y3JuYy1vbXMtc2FsZXMtZTJlLXRlc3RzLWtleS0wMDE=";
    public const string JwtIssuer = "OmsCrncAuthServer";
    public const string JwtAudience = "OmsCrncApis";
}

/// <summary>Числовые значения enum'ов контракта — намеренно int, а не enum:
/// тесты проверяют в том числе то, что API отдаёт и принимает именно числа.</summary>
internal static class JobTypes
{
    public const int New = 1;
    public const int Repair = 2;
}

internal static class OrderStatuses
{
    public const int NotSent = 1;
    public const int NeedSignoff = 2;
    public const int Signed = 3;
    public const int ConvertedToJob = 4;
}

internal static class MaterialSources
{
    public const int ToBeOrdered = 1;
    public const int Stock = 3;
}

internal static class SignoffTypes
{
    public const int Email = 1;
}

public sealed record CreateOrderRequest(
    int JobType,
    string JobDescription,
    string CustomerTitle,
    string CustomerAbbreviation,
    string CustomerContactPersonFirstName,
    string CustomerContactPersonLastName,
    string CustomerContactPersonEmail,
    string CustomerContactPersonPhone);

public sealed record CreateOrderResponse(Guid Id);

public sealed record EditOrderRequest(
    Guid Id,
    int JobType,
    string JobDescription,
    int Status,
    int? MaterialSource,
    int? SignoffType,
    string CustomerTitle,
    string CustomerAbbreviation,
    string CustomerContactPersonFirstName,
    string CustomerContactPersonLastName,
    string CustomerContactPersonEmail,
    string CustomerContactPersonPhone);

public sealed record TextValueResponse(int Value, string Text);

public sealed record GetOrderResponse(
    Guid Id,
    int Status,
    List<TextValueResponse> Statuses,
    string DateCreated,
    int JobType,
    List<TextValueResponse> JobTypes,
    int? MaterialSource,
    List<TextValueResponse> MaterialSources,
    int? SignoffType,
    List<TextValueResponse> SignoffTypes,
    string JobDescription,
    Guid? JobId,
    string? JobNumber,
    string CustomerContactPersonFirstName,
    string CustomerContactPersonLastName,
    string CustomerTitle,
    string CustomerAbbreviation,
    string CustomerContactPersonEmail,
    string CustomerContactPersonPhone,
    string DateSentToCustomer);

public sealed record GetNewOrderResponse(
    int JobType,
    List<TextValueResponse> JobTypes,
    string JobDescription,
    string CustomerContactPersonFirstName,
    string CustomerContactPersonLastName,
    string CustomerTitle,
    string CustomerAbbreviation,
    string CustomerContactPersonEmail,
    string CustomerContactPersonPhone);

public sealed record OrdersTableItemResponse(
    Guid Id,
    string Number,
    string CreatedDate,
    string JobType,
    string JobDescription,
    string DateSentToCustomer,
    string Customer,
    string CustomerSignOffType,
    string Status,
    int StatusEnum);

public sealed record OrdersTableResponse(List<OrdersTableItemResponse> Items);

/// <summary>Форма ответа Security на <c>GET /api/users?roles=...</c> — её отдаёт заглушка.</summary>
public sealed record UserItemStub(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Login,
    string Password,
    string? Phone,
    Guid RoleId,
    string Role,
    bool IsActive)
{
    public static UserItemStub MainManager(string login = "main_manager") => new(
        Id: Guid.NewGuid(),
        FirstName: "Main",
        LastName: "Manager",
        FullName: "Main Manager",
        Email: $"{login}@crnc.com",
        Login: login,
        Password: "111111",
        Phone: null,
        RoleId: Guid.Parse("f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293"),
        Role: "Main manager",
        IsActive: true);
}
