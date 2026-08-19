using System.Text.Json;

namespace Crnc.Oms.Production.E2ETests;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Значения, которые <c>ProductionDbInitializer</c> кладёт в базу при каждом старте
/// сервиса, и настройки аутентификации, которые фикстура передаёт контейнеру.
/// </summary>
internal static class SeedData
{
    public static readonly Guid SeededJobId = Guid.Parse("f425e777-1d53-40d3-99dd-d51e1a72fafa");
    public static readonly Guid SeededOrderId = Guid.Parse("5c5c6017-1b1f-4a46-b423-455ad4f273fe");
    public const string SeededOrderNumber = "5c5c6017";
    public const string SeededManager = "Shon Bean (shon_bean)";

    // Собственный ключ подписи тестов: фикстура задаёт его контейнеру через
    // Auth:JwtBase64SymmetricKey, поэтому набор не зависит ни от значения в
    // appsettings.json (оно уже выровнено с Security), ни от будущей ротации.
    public const string JwtBase64SymmetricKey = "Y3JuYy1vbXMtcHJvZHVjdGlvbi1lMmUtdGVzdHMta2V5LTAwMQ==";
    public const string JwtIssuer = "OmsCrncAuthServer";
    public const string JwtAudience = "OmsCrncApis";
}

/// <summary>Числовые значения PriorityEnum - намеренно int, а не enum: тесты
/// проверяют в том числе то, что API отдаёт и принимает именно числа.
/// Priority - единственный enum контракта, доступный на вход через HTTP
/// (PUT /{id}/priority); JobType/MaterialSource в JSON приезжают уже как
/// строки-Description (см. DisplayNames ниже), их числовых кодов извне не видно.</summary>
internal static class Priorities
{
    public const int High = 1;
    public const int Middle = 2;
    public const int Low = 3;
}

/// <summary>
/// Точные имена членов enum'ов JobType/MaterialSource - то, что нужно положить в
/// OrderConvertedToJobEvent.JobType/.MaterialSource, потому что
/// OrderConvertedToJobConsumer разбирает их через Enum.Parse&lt;T&gt;(строка).
/// Не путать с DisplayNames ниже - Enum.Parse требует имя члена, а не [Description].
/// </summary>
internal static class EnumMemberNames
{
    public const string JobTypeNew = "New";
    public const string JobTypeRepair = "Repair";

    public const string MaterialSourceToBeOrdered = "ToBeOrdered";
    public const string MaterialSourceIncludedByCustomer = "IncludedByCustomer";
    public const string MaterialSourceStock = "Stock";
}

/// <summary>
/// Строки, которые JobService кладёт в JSON через EnumHelper.GetDescription - то,
/// что реально приходит в полях jobType/materialSource HTTP-ответа. JobType не
/// помечен [Description], поэтому GetDescription для него совпадает с именем члена
/// (JobTypeNew совпадает с EnumMemberNames.JobTypeNew); у MaterialSource - нет.
/// </summary>
internal static class DisplayNames
{
    public const string JobTypeNew = "New";
    public const string MaterialSourceIncludedByCustomer = "Included by customer";
    public const string MaterialSourceToBeOrdered = "To be ordered";
}

public sealed record TextValueResponse(int Value, string Text);

public sealed record JobListItemResponse(
    Guid Id,
    string Number,
    string DateCreated,
    string JobType,
    string JobDescription,
    string MaterialSource,
    string Manager,
    string Priority,
    int PriorityEnum,
    bool IsJobCompeted);

public sealed record JobsForListResponse(List<JobListItemResponse> Items);

public sealed record JobResponse(
    Guid Id,
    string Number,
    string DateCreated,
    string JobType,
    string JobDescription,
    bool IsJobCompeted,
    string MaterialSource,
    string Manager,
    int PriorityEnum,
    List<TextValueResponse> Priorities);

public sealed record ChangePriorityRequest(int Priority);
