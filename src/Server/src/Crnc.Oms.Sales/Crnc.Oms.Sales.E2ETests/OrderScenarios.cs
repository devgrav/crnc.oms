using System.Net.Http.Json;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Общие шаги, которые нужны почти каждому write-тесту: завести собственный заказ
/// и собрать корректный payload для PUT. Сид-заказ трогают только read-тесты,
/// всё остальное работает на своих заказах — фикстура одна на всю коллекцию.
/// </summary>
internal static class OrderScenarios
{
    public static CreateOrderRequest NewOrderRequest(string? descriptionOverride = null) => new(
        JobType: JobTypes.New,
        JobDescription: descriptionOverride ?? $"e2e job {Guid.NewGuid():N}",
        CustomerTitle: $"Customer {Guid.NewGuid():N}",
        CustomerAbbreviation: "E2",
        CustomerContactPersonFirstName: "John",
        CustomerContactPersonLastName: "Galt",
        CustomerContactPersonEmail: $"{Guid.NewGuid():N}@e2e.test",
        CustomerContactPersonPhone: "+79151211112");

    public static async Task<Guid> CreateOrderAsync(HttpClient client, CreateOrderRequest? request = null)
    {
        var response = await client.PostAsJsonAsync("api/orders", request ?? NewOrderRequest(), JsonDefaults.Options);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonDefaults.Options);

        return created!.Id;
    }

    /// <summary>
    /// PUT требует полного payload. MaterialSource и SignoffType помечены [EnumRequired],
    /// который отвергает null, — поэтому они заполняются всегда, даже когда тесту безразличны.
    /// </summary>
    public static EditOrderRequest EditRequest(
        Guid orderId,
        int status,
        string? jobDescription = null,
        int materialSource = MaterialSources.Stock,
        int signoffType = SignoffTypes.Email) => new(
        Id: orderId,
        JobType: JobTypes.Repair,
        JobDescription: jobDescription ?? $"edited {Guid.NewGuid():N}",
        Status: status,
        MaterialSource: materialSource,
        SignoffType: signoffType,
        CustomerTitle: $"Customer {Guid.NewGuid():N}",
        CustomerAbbreviation: "ED",
        CustomerContactPersonFirstName: "Jane",
        CustomerContactPersonLastName: "Smith",
        CustomerContactPersonEmail: $"{Guid.NewGuid():N}@e2e.test",
        CustomerContactPersonPhone: "+79151211113");

    public static Task<HttpResponseMessage> EditOrderAsync(HttpClient client, EditOrderRequest request) =>
        client.PutAsJsonAsync("api/orders", request, JsonDefaults.Options);

    public static async Task<GetOrderResponse> GetOrderAsync(HttpClient client, Guid orderId)
    {
        var response = await client.GetAsync($"api/orders/{orderId}");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<GetOrderResponse>(JsonDefaults.Options))!;
    }
}
