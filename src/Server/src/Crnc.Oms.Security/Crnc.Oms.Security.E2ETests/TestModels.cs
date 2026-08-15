using System.Text.Json;

namespace Crnc.Oms.Security.E2ETests;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

internal static class SeedData
{
    public static readonly Guid AdminRoleId = Guid.Parse("0c7871c3-4751-4af6-b0ef-21c38064e9f2");
    public static readonly Guid MainManagerRoleId = Guid.Parse("f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293");
    public static readonly Guid ManagerRoleId = Guid.Parse("29679868-fcfe-4350-913d-526a54ea896d");

    public static readonly Guid AdminUserId = Guid.Parse("2a89985f-f013-4f2a-9545-395efb43a142");
    public const string AdminLogin = "admin";
    public const string AdminPassword = "111111";

    public static readonly Guid ShonBeanUserId = Guid.Parse("b6ba35b2-adff-43a6-9cd7-b408240a6d6f");
    public const string ShonBeanLogin = "shon_bean";
    public const string ShonBeanPassword = "111111";
}

public sealed record AccountRequest(string Login, string Password);

public sealed record CurrentUserResponse(Guid Id, string Login, string FullName, string Role, string Jwt);

public sealed record TextValueResponse(Guid Value, string Text);

public sealed record SaveUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Login,
    string Password,
    string? Phone,
    Guid RoleId,
    string? PhotoBase64 = null,
    string? PhotoMimeType = null,
    bool IsActive = true);

public sealed record UserItemResponse(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string Login,
    string Password,
    string? Phone,
    Guid RoleId,
    string Role,
    string? PhotoBase64,
    string? PhotoMimeType,
    bool IsActive);

public sealed record UserShortInfoResponse(
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
    bool IsActive);
