using System.Net.Http.Json;
using System.Text.Json;

namespace Crnc.Oms.Notification.E2ETests;

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
    /// GET /api/users/{id} → 200 с карточкой пользователя.
    /// <para>
    /// Заголовок Authorization намеренно <b>не</b> требуется: настоящий эндпойнт Security
    /// помечен [AllowAnonymous], и на этом держится разрешение канала доставки — Notification
    /// сам добывает email по UserId, потому что контракт уведомления его не несёт.
    /// Более строгая заглушка проверяла бы контракт, которого нет.
    /// См. раздел «Разрешение канала доставки» в docs/migrations/notification-net10-migration-plan.md.
    /// </para>
    /// </summary>
    public Task StubUserAsync(Guid userId, UserInfoStub user) =>
        AddMappingAsync(new
        {
            request = new { method = "GET", urlPath = $"/api/users/{userId}" },
            response = new
            {
                status = 200,
                jsonBody = user,
                headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
            }
        });

    /// <summary>GET /api/users/{id} → произвольный код, для проверки деградации гейтвея.</summary>
    public Task StubUserFailureAsync(Guid userId, int statusCode) =>
        AddMappingAsync(new
        {
            request = new { method = "GET", urlPath = $"/api/users/{userId}" },
            response = new { status = statusCode }
        });

    /// <summary>Запросы, которые Gateway реально отправил в Security.</summary>
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

    /// <summary>Ждёт появления запроса на путь — исходящий вызов асинхронен относительно ответа.</summary>
    public async Task<List<RecordedRequest>> WaitForRequestsAsync(string urlContains, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (true)
        {
            var requests = await GetRecordedRequestsAsync();
            var matched = requests.Where(r => r.Url.Contains(urlContains, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matched.Count > 0 || DateTime.UtcNow >= deadline)
                return matched;

            await Task.Delay(250);
        }
    }

    private async Task AddMappingAsync(object mapping)
    {
        var response = await _client.PostAsJsonAsync("/__admin/mappings", mapping, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();
    }

    public sealed record RecordedRequest(string Method, string Url, Dictionary<string, string> Headers);
}
