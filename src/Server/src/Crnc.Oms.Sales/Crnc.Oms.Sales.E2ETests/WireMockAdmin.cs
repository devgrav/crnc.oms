using System.Net.Http.Json;
using System.Text.Json;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Обёртка над admin API WireMock — заглушки Security задаются из кода,
/// без монтирования файлов в контейнер.
/// </summary>
public sealed class WireMockAdmin
{
    private readonly HttpClient _client;

    public WireMockAdmin(Uri baseAddress)
    {
        _client = new HttpClient { BaseAddress = baseAddress };
    }

    /// <summary>Сбрасывает и заглушки, и журнал запросов — вызывается в начале каждого теста.</summary>
    public async Task ResetAsync()
    {
        var response = await _client.PostAsync("/__admin/reset", content: null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// GET /api/users?roles=... → 200 со списком пользователей.
    /// EmployeeSecurityGateway ждёт от Security именно эту форму.
    /// </summary>
    public Task StubMainManagersAsync(params UserItemStub[] users) =>
        AddMappingAsync(new
        {
            request = new { method = "GET", urlPath = "/api/users" },
            response = new
            {
                status = 200,
                jsonBody = users,
                headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
            }
        });

    /// <summary>GET /api/users → произвольный код ответа, для проверки деградации гейтвея.</summary>
    public Task StubUsersFailureAsync(int statusCode) =>
        AddMappingAsync(new
        {
            request = new { method = "GET", urlPath = "/api/users" },
            response = new { status = statusCode }
        });

    /// <summary>Запросы, которые Sales реально отправил в Security.</summary>
    public async Task<List<RecordedRequest>> GetRecordedRequestsAsync()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/__admin/requests", JsonDefaults.Options);

        var requests = new List<RecordedRequest>();

        foreach (var entry in json.GetProperty("requests").EnumerateArray())
        {
            var request = entry.GetProperty("request");

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (request.TryGetProperty("headers", out var headersElement))
            {
                foreach (var header in headersElement.EnumerateObject())
                    headers[header.Name] = header.Value.ToString();
            }

            requests.Add(new RecordedRequest(
                request.GetProperty("method").GetString()!,
                request.GetProperty("url").GetString()!,
                headers));
        }

        return requests;
    }

    private async Task AddMappingAsync(object mapping)
    {
        var response = await _client.PostAsJsonAsync("/__admin/mappings", mapping, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();
    }

    public sealed record RecordedRequest(string Method, string Url, Dictionary<string, string> Headers);
}
