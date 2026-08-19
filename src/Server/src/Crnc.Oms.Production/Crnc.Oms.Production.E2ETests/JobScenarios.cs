using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Production.E2ETests;

/// <summary>
/// Общие сценарии для тестов чтения/записи: создание работы идёт не через HTTP
/// (эндпоинта нет), а публикацией OrderConvertedToJobEvent - см. решение 8 плана.
/// </summary>
internal static class JobScenarios
{
    public static async Task<Guid> CreateJobAsync(
        ProductionApiFixture fixture,
        string jobType = EnumMemberNames.JobTypeNew,
        string materialSource = EnumMemberNames.MaterialSourceIncludedByCustomer,
        string jobDescription = "e2e test job",
        string managerFullName = "E2e Manager",
        string managerLogin = "e2e_manager")
    {
        var orderId = Guid.NewGuid();
        var orderNumber = orderId.ToString("N")[..8];

        await fixture.PublishOrderConvertedToJobAsync(
            orderId, orderNumber, jobType, jobDescription, materialSource, managerFullName, managerLogin);

        var snapshot = await fixture.WaitForJobCreatedAsync(orderId);
        snapshot.Should().NotBeNull("Production должен ответить JobCreatedForOrderEvent на конвертацию заказа");

        // Квирк текущей реализации (JobService.CreateJob): Id работы совпадает с OrderId.
        return snapshot!.JobId;
    }

    public static async Task<JobResponse> GetJobAsync(HttpClient client, Guid jobId)
    {
        var response = await client.GetAsync($"api/jobs/{jobId}");
        response.EnsureSuccessStatusCode();

        var job = await response.Content.ReadFromJsonAsync<JobResponse>(JsonDefaults.Options);
        return job!;
    }
}
