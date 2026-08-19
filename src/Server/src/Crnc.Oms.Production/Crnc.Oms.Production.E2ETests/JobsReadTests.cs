using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Crnc.Oms.Production.E2ETests;

[Collection(ProductionApiCollection.Name)]
public sealed class JobsReadTests
{
    // JobService форматирует даты через DateTimeExtensions.ToStandartFormatWithTime()
    // как "dd.MM.yyy hh:mm:ss" - три "y" (при годе >= 1000 равнозначно "yyyy" в .NET)
    // и 12-часовой "hh" без AM/PM. Предсуществующее поведение, миграция его не меняет -
    // см. план, раздел "Что покрываем".
    private static readonly Regex DateWithSeconds = new(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}$");

    private readonly ProductionApiFixture _fixture;

    public JobsReadTests(ProductionApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetJobs_AsAuthorizedUser_ReturnsSeededJob()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.GetAsync("api/jobs");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jobs = await response.Content.ReadFromJsonAsync<JobsForListResponse>(JsonDefaults.Options);
        var seeded = jobs!.Items.Should().ContainSingle(x => x.Id == SeedData.SeededJobId).Subject;

        seeded.Number.Should().Be("f425e777");
        seeded.Manager.Should().Be(SeedData.SeededManager);
        seeded.JobType.Should().Be(DisplayNames.JobTypeNew);
        seeded.MaterialSource.Should().Be(DisplayNames.MaterialSourceIncludedByCustomer);
        seeded.Priority.Should().Be("Low");
        seeded.PriorityEnum.Should().Be(Priorities.Low);
        seeded.IsJobCompeted.Should().BeFalse();
        seeded.DateCreated.Should().MatchRegex(DateWithSeconds);
    }

    [Fact]
    public async Task GetJobs_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.GetAsync("api/jobs");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetJobById_SeededJob_ReturnsJobWithAvailablePriorities()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var job = await JobScenarios.GetJobAsync(client, SeedData.SeededJobId);

        //Assert
        job.Id.Should().Be(SeedData.SeededJobId);
        job.Manager.Should().Be(SeedData.SeededManager);
        job.JobType.Should().Be(DisplayNames.JobTypeNew);
        job.MaterialSource.Should().Be(DisplayNames.MaterialSourceIncludedByCustomer);
        job.PriorityEnum.Should().Be(Priorities.Low);
        job.IsJobCompeted.Should().BeFalse();
        job.DateCreated.Should().MatchRegex(DateWithSeconds);

        // ToDictionaryWithKeysAndDescriptions(Priority.Low) обходит все значения enum'а,
        // а не только переданное - три доступных приоритета вне зависимости от текущего.
        job.Priorities.Should().BeEquivalentTo(new[]
        {
            new TextValueResponse(Priorities.High, "High"),
            new TextValueResponse(Priorities.Middle, "Middle"),
            new TextValueResponse(Priorities.Low, "Low")
        });
    }

    [Fact]
    public async Task GetJobById_UnknownId_ReturnsNotFound()
    {
        //Arrange - в отличие от FinishJob/ChangePriority (§7 плана), GetJob
        //проверяет результат FindByIdAsync и бросает MissingEntityException, которую
        //контроллер ловит и превращает в 404 - это уже правильное поведение.
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.GetAsync($"api/jobs/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetJobById_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.GetAsync($"api/jobs/{SeedData.SeededJobId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
