using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Crnc.Oms.Production.E2ETests;

[Collection(ProductionApiCollection.Name)]
public sealed class JobsWriteTests
{
    private readonly ProductionApiFixture _fixture;

    public JobsWriteTests(ProductionApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FinishJob_ExistingJob_MarksCompleted()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();
        var jobId = await JobScenarios.CreateJobAsync(_fixture);

        //Act
        var response = await client.PutAsync($"api/jobs/{jobId}/finished", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var job = await JobScenarios.GetJobAsync(client, jobId);
        job.IsJobCompeted.Should().BeTrue();
    }

    [Fact]
    public async Task FinishJob_UnknownId_ReturnsNotFound()
    {
        //Arrange - до фазы 5 плана миграции JobService.FinishJob не проверяло
        //результат FindByIdAsync и падало с NullReferenceException (500), хотя
        //контроллер декларировал и уже ловил 404. Починено вместе с этим тестом.
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.PutAsync($"api/jobs/{Guid.NewGuid()}/finished", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FinishJob_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.PutAsync($"api/jobs/{SeedData.SeededJobId}/finished", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePriority_ExistingJob_UpdatesPriority()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();
        var jobId = await JobScenarios.CreateJobAsync(_fixture);

        //Act
        var response = await client.PutAsJsonAsync(
            $"api/jobs/{jobId}/priority", new ChangePriorityRequest(Priorities.High), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var job = await JobScenarios.GetJobAsync(client, jobId);
        job.PriorityEnum.Should().Be(Priorities.High);
    }

    [Fact]
    public async Task ChangePriority_ValidPayload_SerializesPriorityEnumAsNumber()
    {
        //Arrange - регресс-тест под §4 плана: PriorityEnum сейчас едет числом
        //(StringEnumConverter здесь никогда не регистрировался), и SPA
        //(components/jobs/priority.ts) ждёт именно число. При переходе на
        //System.Text.Json JsonStringEnumConverter добавлять нельзя.
        using var client = _fixture.CreateAuthorizedClient();
        var jobId = await JobScenarios.CreateJobAsync(_fixture);

        //Act
        var response = await client.GetAsync($"api/jobs/{jobId}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        //Assert
        json.GetProperty("priorityEnum").ValueKind.Should().Be(JsonValueKind.Number);
    }

    [Fact]
    public async Task ChangePriority_UnknownId_ReturnsNotFound()
    {
        //Arrange - см. FinishJob_UnknownId_ReturnsNotFound: тот же фикс, тот же коммит.
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.PutAsJsonAsync(
            $"api/jobs/{Guid.NewGuid()}/priority",
            new ChangePriorityRequest(Priorities.High),
            JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePriority_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.PutAsJsonAsync(
            $"api/jobs/{SeedData.SeededJobId}/priority",
            new ChangePriorityRequest(Priorities.High),
            JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
