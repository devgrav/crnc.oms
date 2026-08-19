using Crnc.Oms.Messaging.Contract.Events;

namespace Crnc.Oms.Production.E2ETests;

/// <summary>
/// Тест играет роль Sales, публикуя это событие вместо него - см. решение 8 и раздел
/// «Пререквизит» плана миграции. Интерфейс приходит по ProjectReference на
/// Crnc.Oms.Production.Messaging.Contract; реализация нужна только затем, что
/// Publish&lt;T&gt; требует конкретный экземпляр. Форма один в один со
/// Crnc.Oms.Sales.Integration.Dto.OrderConvertedToJobEventDto - тем, что реально
/// шлёт Sales в проде.
/// </summary>
public sealed class OrderConvertedToJobEventDto : OrderConvertedToJobEvent
{
    // Интерфейс объявляет { get; set; } - init-only свойства его не реализуют.
    public required string JobType { get; set; }
    public required string JobDescription { get; set; }
    public required string MaterialSource { get; set; }
    public required string ManagerFullName { get; set; }
    public required string ManagerLogin { get; set; }
    public required Guid OrderId { get; set; }
    public required string OrderNumber { get; set; }
}
