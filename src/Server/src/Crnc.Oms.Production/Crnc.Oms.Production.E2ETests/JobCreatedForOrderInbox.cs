using System.Collections.Concurrent;
using Crnc.Oms.Messaging.Contract.Events;

namespace Crnc.Oms.Production.E2ETests;

public sealed record JobCreatedForOrderSnapshot(Guid JobId, string JobNumber, Guid OrderId);

/// <summary>
/// Тест играет роль Sales и на приёмной стороне: собственный MassTransit-бас держит
/// receive endpoint, подписанный на JobCreatedForOrderEvent, и складывает сюда всё,
/// что приходит. Снимок делается сразу в обработчике - хранить сам интерфейсный
/// прокси-объект MassTransit дольше времени обработки сообщения не нужно.
/// </summary>
public sealed class JobCreatedForOrderInbox
{
    private readonly ConcurrentDictionary<Guid, JobCreatedForOrderSnapshot> _byOrderId = new();

    public void Add(JobCreatedForOrderEvent message)
    {
        _byOrderId[message.OrderId] = new JobCreatedForOrderSnapshot(message.JobId, message.JobNumber, message.OrderId);
    }

    /// <summary>Публикация асинхронна относительно ответа HTTP/шины, поэтому без
    /// ожидания тесты флакают - опрос с интервалом, как WaitForMessagesAsync в Sales.</summary>
    public async Task<JobCreatedForOrderSnapshot?> WaitForAsync(Guid orderId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (_byOrderId.TryGetValue(orderId, out var snapshot))
                return snapshot;

            await Task.Delay(250);
        }

        return _byOrderId.TryGetValue(orderId, out var last) ? last : null;
    }

    /// <summary>Для проверки идемпотентности: убеждаемся, что второе событие для того
    /// же OrderId не пришло, дав ему разумное время дойти, если бы оно было отправлено.</summary>
    public async Task<bool> StaysAbsentAsync(Guid orderId, TimeSpan window)
    {
        await Task.Delay(window);
        return !_byOrderId.ContainsKey(orderId);
    }
}
