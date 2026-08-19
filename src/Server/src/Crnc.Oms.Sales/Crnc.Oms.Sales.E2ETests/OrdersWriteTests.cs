using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Crnc.Oms.Sales.E2ETests;

[Collection(SalesApiCollection.Name)]
public sealed class OrdersWriteTests
{
    private static readonly Regex DateWithMinutes = new(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}$");

    private readonly SalesApiFixture _fixture;

    public OrdersWriteTests(SalesApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateOrder_ValidPayload_PersistsOrderReadableBack()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();
        var request = OrderScenarios.NewOrderRequest();

        //Act
        var response = await client.PostAsJsonAsync("api/orders", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<CreateOrderResponse>(JsonDefaults.Options);
        created!.Id.Should().NotBeEmpty();

        var order = await OrderScenarios.GetOrderAsync(client, created.Id);
        order.JobDescription.Should().Be(request.JobDescription);
        order.CustomerTitle.Should().Be(request.CustomerTitle);
        order.CustomerAbbreviation.Should().Be(request.CustomerAbbreviation);
        order.CustomerContactPersonEmail.Should().Be(request.CustomerContactPersonEmail);
        order.CustomerContactPersonPhone.Should().Be(request.CustomerContactPersonPhone);
        order.Status.Should().Be(OrderStatuses.NotSent, "новый заказ начинает жизнь в статусе Not sent");
    }

    [Fact]
    public async Task CreateOrder_ValidPayload_StoresAndFormatsDates()
    {
        //Arrange - регресс-тест под §5.1 плана миграции: Npgsql 6+ маппит DateTime на
        //timestamptz и запрещает запись Kind=Local, а весь домен живёт на DateTime.Now.
        //Если провайдер начнёт ругаться, этот тест упадёт первым.
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        //Assert
        var order = await OrderScenarios.GetOrderAsync(client, orderId);
        order.DateCreated.Should().MatchRegex(DateWithMinutes);
        order.DateCreated.Should().StartWith(DateTime.Now.ToString("dd.MM.yyyy"));
    }

    [Fact]
    public async Task CreateOrder_ValidPayload_SerializesEnumsAsNumbers()
    {
        //Arrange - регресс-тест под §4 плана: в Sales enum'ы контракта ездят числами
        //(StringEnumConverter здесь никогда не регистрировался), и SPA ждёт именно числа.
        //При переходе на System.Text.Json JsonStringEnumConverter добавлять нельзя.
        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);

        //Act
        var response = await client.GetAsync($"api/orders/{orderId}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        //Assert
        json.GetProperty("status").ValueKind.Should().Be(JsonValueKind.Number);
        json.GetProperty("jobType").ValueKind.Should().Be(JsonValueKind.Number);
        json.GetProperty("statuses")[0].GetProperty("value").ValueKind.Should().Be(JsonValueKind.Number);
    }

    [Fact]
    public async Task CreateOrder_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys()
    {
        //Arrange - регресс-тест под §4 плана: PropertyNamingPolicy в System.Text.Json
        //не распространяется на ключи словаря, нужен отдельный DictionaryKeyPolicy.
        //Без него ключи ошибок валидации молча уедут в PascalCase.
        using var client = _fixture.CreateAuthorizedClient();
        var request = OrderScenarios.NewOrderRequest() with { JobDescription = null! };

        //Act
        var response = await client.PostAsJsonAsync("api/orders", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = await ReadValidationErrorsAsync(response);
        errors.TryGetProperty("jobDescription", out _).Should().BeTrue(
            "ключи ошибок валидации должны оставаться в camelCase");
        errors.TryGetProperty("JobDescription", out _).Should().BeFalse(
            "ключи ошибок валидации не должны регрессировать в PascalCase");
    }

    [Fact]
    public async Task CreateOrder_InvalidAbbreviation_ReturnsBadRequest()
    {
        //Arrange - NameAbbreviation в домене требует ровно два символа.
        using var client = _fixture.CreateAuthorizedClient();
        var request = OrderScenarios.NewOrderRequest() with { CustomerAbbreviation = "TOO-LONG" };

        //Act
        var response = await client.PostAsJsonAsync("api/orders", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = await ReadValidationErrorsAsync(response);
        errors.TryGetProperty("customerAbbreviation", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrder_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.PostAsJsonAsync(
            "api/orders", OrderScenarios.NewOrderRequest(), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditOrder_ChangesOrderAndStatus_Persists()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync();

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);
        var edit = OrderScenarios.EditRequest(orderId, OrderStatuses.NeedSignoff);

        //Act
        var response = await OrderScenarios.EditOrderAsync(client, edit);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await OrderScenarios.GetOrderAsync(client, orderId);
        order.Status.Should().Be(OrderStatuses.NeedSignoff);
        order.JobType.Should().Be(JobTypes.Repair);
        order.JobDescription.Should().Be(edit.JobDescription);
        order.CustomerTitle.Should().Be(edit.CustomerTitle);
        order.MaterialSource.Should().Be(edit.MaterialSource);
        order.SignoffType.Should().Be(edit.SignoffType);
        order.DateSentToCustomer.Should().MatchRegex(DateWithMinutes,
            "переход в NeedSignoff проставляет дату отправки клиенту");
    }

    [Fact]
    public async Task EditOrder_UnknownId_ReturnsNotFound()
    {
        //Arrange
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync();

        using var client = _fixture.CreateAuthorizedClient();
        var edit = OrderScenarios.EditRequest(Guid.NewGuid(), OrderStatuses.NeedSignoff);

        //Act
        var response = await OrderScenarios.EditOrderAsync(client, edit);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EditOrder_WithoutMaterialSource_ReturnsBadRequest()
    {
        //Arrange - [EnumRequired] отвергает null, поэтому materialSource обязателен на любом
        //переходе. Побочное следствие: доменная проверка в Order.ConvertToJob() через API
        //недостижима - до неё запрос не доходит. Фиксируем текущее поведение перед миграцией.
        await _fixture.SecurityStub.ResetAsync();
        await _fixture.SecurityStub.StubMainManagersAsync();

        using var client = _fixture.CreateAuthorizedClient();
        var orderId = await OrderScenarios.CreateOrderAsync(client);
        var edit = OrderScenarios.EditRequest(orderId, OrderStatuses.ConvertedToJob) with { MaterialSource = null };

        //Act
        var response = await OrderScenarios.EditOrderAsync(client, edit);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = await ReadValidationErrorsAsync(response);
        errors.TryGetProperty("materialSource", out _).Should().BeTrue();
    }

    /// <summary>
    /// [ApiController] отдаёт ValidationProblemDetails, где ошибки лежат в объекте "errors".
    /// Разворачиваем его, если он есть, чтобы тесты проверяли сами ключи, а не обёртку.
    /// </summary>
    private static async Task<JsonElement> ReadValidationErrorsAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);

        return json.TryGetProperty("errors", out var errors) ? errors : json;
    }
}
