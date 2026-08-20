using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Crnc.Oms.Notification.E2ETests;

/// <summary>
/// Обёртка над management API RabbitMQ. Нужна для трёх вещей: подготовить «шпионские»
/// очереди до действия, посчитать в них сообщения после, и положить команду в очередь
/// руками — так набор играет роль отсутствующего Sales, не поднимая собственную шину.
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
    /// Send в MassTransit идёт в fanout-exchange, одноимённый очереди получателя. Своя
    /// очередь, привязанная к тому же exchange, получает копию каждой команды и не мешает
    /// настоящему консьюмеру. Exchange объявляем сами (fanout, durable — те же параметры,
    /// что ставит MassTransit), поэтому порядок старта контейнеров не важен.
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

    /// <summary>
    /// Забирает из очереди всё, что в ней лежит, и возвращает тела сообщений.
    /// <para>
    /// Считать сообщения дельтами «было/стало» здесь нельзя: фикстура общая, отправка
    /// асинхронна относительно ответа HTTP, и опоздавшее сообщение соседнего теста
    /// сдвигает счётчик. Поэтому тесты ищут в очереди <b>своё</b> сообщение по уникальному
    /// маркеру, а чужие отбрасывают — очередь заодно самоочищается.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<string>> DrainAsync(string queue, int count = 100)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/queues/{DefaultVirtualHost}/{Uri.EscapeDataString(queue)}/get",
            new { count, ackmode = "ack_requeue_false", encoding = "auto" },
            JsonDefaults.Options);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        return json.EnumerateArray()
            .Select(m => m.TryGetProperty("payload", out var payload) ? payload.GetString() ?? string.Empty : string.Empty)
            .ToList();
    }

    /// <summary>
    /// Ждёт появления в очереди сообщения, чьё тело содержит маркер. Всё остальное,
    /// что попалось по пути, отбрасывается — это следы соседних тестов.
    /// </summary>
    public async Task<string?> WaitForMessageAsync(string queue, string marker, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (true)
        {
            foreach (var payload in await DrainAsync(queue))
            {
                if (payload.Contains(marker, StringComparison.Ordinal))
                    return payload;
            }

            if (DateTime.UtcNow >= deadline)
                return null;

            await Task.Delay(250);
        }
    }

    /// <summary>
    /// Убеждается, что сообщение с маркером в очередь <b>не</b> попало. Ждёт указанное
    /// время, отбрасывая чужие сообщения, и возвращает false, если своё так и не пришло.
    /// </summary>
    public async Task<bool> AnyMessageAsync(string queue, string marker, TimeSpan window)
    {
        return await WaitForMessageAsync(queue, marker, window) is not null;
    }

    /// <summary>
    /// Кладёт команду в очередь так, как это сделал бы MassTransit: конверт с messageType
    /// и content-type application/vnd.masstransit+json.
    /// <para>
    /// Это единственный способ проверить вход сервиса из шины, не поднимая в тестовом
    /// процессе собственный бус и не заводя ProjectReference на контракты сервиса.
    /// Формат конверта проверен на живом стенде: MassTransit 6 его принимает.
    /// </para>
    /// </summary>
    public async Task PublishCommandAsync(string exchange, string messageType, object message)
    {
        var envelope = new
        {
            messageId = Guid.NewGuid().ToString(),
            conversationId = Guid.NewGuid().ToString(),
            sourceAddress = "rabbitmq://message-broker/e2e-tests",
            destinationAddress = $"rabbitmq://message-broker/{exchange}",
            messageType = new[] { messageType },
            message,
            sentTime = DateTime.UtcNow.ToString("O")
        };

        var body = new
        {
            properties = new { content_type = "application/vnd.masstransit+json", delivery_mode = 2 },
            routing_key = "",
            payload = JsonSerializer.Serialize(envelope, JsonDefaults.Options),
            payload_encoding = "string"
        };

        var response = await _client.PostAsJsonAsync(
            $"/api/exchanges/{DefaultVirtualHost}/{Uri.EscapeDataString(exchange)}/publish",
            body,
            JsonDefaults.Options);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);
        if (!result.GetProperty("routed").GetBoolean())
            throw new InvalidOperationException($"Команда не была маршрутизирована exchange'ем {exchange}.");
    }

    /// <summary>Есть ли очередь с таким именем — чтобы ловить появление *_error.</summary>
    public async Task<IReadOnlyList<string>> GetQueueNamesAsync()
    {
        var response = await _client.GetAsync($"/api/queues/{DefaultVirtualHost}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        return json.EnumerateArray()
            .Select(q => q.GetProperty("name").GetString()!)
            .ToList();
    }

    private async Task PutAsync(string url, object body)
    {
        var response = await _client.PutAsJsonAsync(url, body, JsonDefaults.Options);
        response.EnsureSuccessStatusCode();
    }
}
