using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Production.E2ETests;

/// <summary>
/// Единственный вход Production - сообщение, не HTTP: работу нельзя создать иначе,
/// чем публикацией OrderConvertedToJobEvent (решение 8 плана миграции). Здесь тест
/// играет роль Sales на обоих концах - публикует событие конвертации и слушает
/// ответное JobCreatedForOrderEvent через собственный бас фикстуры.
/// </summary>
[Collection(ProductionApiCollection.Name)]
public sealed class JobMessagingTests
{
    private readonly ProductionApiFixture _fixture;

    public JobMessagingTests(ProductionApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OrderConvertedToJobEvent_NewOrder_CreatesJobAndRepliesWithJobCreatedEvent()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var orderNumber = orderId.ToString("N")[..8];

        //Act
        await _fixture.PublishOrderConvertedToJobAsync(
            orderId,
            orderNumber,
            EnumMemberNames.JobTypeNew,
            "e2e messaging test job",
            EnumMemberNames.MaterialSourceToBeOrdered,
            "E2e Manager",
            "e2e_manager");

        var snapshot = await _fixture.WaitForJobCreatedAsync(orderId);

        //Assert
        snapshot.Should().NotBeNull(
            "публикация OrderConvertedToJobEvent должна создать работу и вернуть JobCreatedForOrderEvent");
        snapshot!.OrderId.Should().Be(orderId);
        // JobService.CreateJob использует dto.OrderId в качестве Id самой работы -
        // квирк текущей реализации, фиксируем как есть (см. план, "Что покрываем").
        snapshot.JobId.Should().Be(orderId);

        using var client = _fixture.CreateAuthorizedClient();
        var job = await JobScenarios.GetJobAsync(client, snapshot.JobId);
        job.JobType.Should().Be(DisplayNames.JobTypeNew);
        job.MaterialSource.Should().Be(DisplayNames.MaterialSourceToBeOrdered);
        job.Manager.Should().Be("E2e Manager (e2e_manager)");
    }

    [Fact]
    public async Task OrderConvertedToJobEvent_PublishedTwiceForSameOrder_IsIdempotent()
    {
        //Arrange - JobService.CreateJob возвращает существующую работу по OrderId без
        //повторной публикации JobCreatedForOrderEvent (ранний return на найденной
        //работе). Реальная идемпотентность, закрепляем её e2e-тестом.
        var orderId = Guid.NewGuid();
        var orderNumber = orderId.ToString("N")[..8];

        Task PublishAsync() => _fixture.PublishOrderConvertedToJobAsync(
            orderId,
            orderNumber,
            EnumMemberNames.JobTypeNew,
            "e2e idempotency test job",
            EnumMemberNames.MaterialSourceStock,
            "E2e Manager",
            "e2e_manager");

        //Act
        await PublishAsync();
        var first = await _fixture.WaitForJobCreatedAsync(orderId);
        first.Should().NotBeNull();

        await PublishAsync();
        // Второго ответного события по конструкции не будет - вместо ожидания
        // несуществующего события даём разумное окно и проверяем результат.
        await Task.Delay(TimeSpan.FromSeconds(3));

        //Assert
        using var client = _fixture.CreateAuthorizedClient();
        var jobs = await client.GetFromJsonAsync<JobsForListResponse>("api/jobs", JsonDefaults.Options);
        jobs!.Items.Should().ContainSingle(x => x.Id == orderId,
            "повторная публикация для того же OrderId не должна создавать вторую работу");
    }
}
