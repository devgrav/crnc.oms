using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Crnc.Oms.Sales.E2ETests;

/// <summary>
/// Обёртка над management API RabbitMQ. Нужна для двух вещей: подготовить
/// «шпионские» очереди до действия и посчитать в них сообщения после.
/// MassTransit в тестовый проект специально не тащим — периметр набора
/// заканчивается на факте отправки, разбирать конверт не требуется.
/// </summary>
public sealed class RabbitMqAdmin
{
    private const string DefaultVirtualHost = "%2F";

    private readonly HttpClient _client;

    public RabbitMqAdmin(Uri baseAddress)
    {
        _client = new HttpClient { BaseAddress = baseAddress };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest")));
    }

    /// <summary>
    /// Объявляет очередь и цепляет её к exchange сообщения.
    /// <para>
    /// Без этого проверять нечего: и Send, и Publish в MassTransit идут в fanout-exchange,
    /// а <c>Publish</c> без единого подписчика брокер просто отбрасывает — счётчик никуда
    /// не вырастет, и тест был бы зелёным всегда. Exchange объявляем сами (fanout, durable —
    /// те же параметры, что ставит MassTransit), поэтому порядок старта контейнеров не важен.
    /// </para>
    /// </summary>
    public async Task EnsureSpyQueueAsync(string exchange, string queue)
    {
        await PutAsync($"/api/exchanges/{DefaultVirtualHost}/{Uri.EscapeDataString(exchange)}",
            new { type = "fanout", durable = true, auto_delete = false });

        await PutAsync($"/api/queues/{DefaultVirtualHost}/{Uri.EscapeDataString(queue)}",
            new { durable = true, auto_delete = false });

        var response = await _client.PostAsJsonAsync(
            $"/api/bindings/{DefaultVirtualHost}/e/{Uri.EscapeDataString(exchange)}/q/{Uri.EscapeDataString(queue)}",
            new { routing_key = "" },
            JsonDefaults.Options);

        response.EnsureSuccessStatusCode();
    }

    public async Task<int> GetMessageCountAsync(string queue)
    {
        var response = await _client.GetAsync($"/api/queues/{DefaultVirtualHost}/{Uri.EscapeDataString(queue)}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        return json.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Number
            ? messages.GetInt32()
            : 0;
    }

    /// <summary>
    /// Ждёт, пока в очереди станет больше сообщений, чем было до действия.
    /// Публикация асинхронна относительно ответа HTTP, поэтому без ожидания тест флакает.
    /// </summary>
    public async Task<int> WaitForMessagesAsync(string queue, int moreThan, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        var count = await GetMessageCountAsync(queue);

        while (count <= moreThan && DateTime.UtcNow < deadline)
        {
            await Task.Delay(250);
            count = await GetMessageCountAsync(queue);
        }

        return count;
    }

    private async Task PutAsync(string url, object body)
    {
        var response = await _client.PutAsJsonAsync(url, body, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();
    }
}
