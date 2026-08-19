using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Crnc.Oms.Sales.E2ETests;

[Collection(SalesApiCollection.Name)]
public sealed class OrdersReadTests
{
    // GetOrderHandler форматирует дату как "dd.MM.yyyy HH:mm",
    // а список заказов - через DateTimeExtensions как "dd.MM.yyy hh:mm:ss".
    private static readonly Regex DateWithMinutes = new(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}$");
    private static readonly Regex DateWithSeconds = new(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}$");

    private readonly SalesApiFixture _fixture;

    public OrdersReadTests(SalesApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetOrders_AsAuthorizedManager_ReturnsSeededOrder()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.GetAsync("api/orders");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orders = await response.Content.ReadFromJsonAsync<OrdersTableResponse>(JsonDefaults.Options);
        var seeded = orders!.Items.Should().ContainSingle(x => x.Id == SeedData.SeededOrderId).Subject;

        seeded.Customer.Should().Be("Some Sales Company");
        seeded.JobType.Should().Be("New");
        seeded.Status.Should().Be("Not sent");
        seeded.StatusEnum.Should().Be(OrderStatuses.NotSent);
        seeded.CreatedDate.Should().MatchRegex(DateWithSeconds);
    }

    [Fact]
    public async Task GetOrders_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.GetAsync("api/orders");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrderById_SeededOrder_ReturnsOrderWithAvailableStatuses()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var order = await OrderScenarios.GetOrderAsync(client, SeedData.SeededOrderId);

        //Assert
        order.Id.Should().Be(SeedData.SeededOrderId);
        order.Status.Should().Be(OrderStatuses.NotSent);
        order.JobType.Should().Be(JobTypes.New);
        order.JobDescription.Should().Be("Develop new wall");
        order.CustomerTitle.Should().Be("Some Sales Company");
        order.CustomerAbbreviation.Should().Be("AS");
        order.CustomerContactPersonEmail.Should().Be("some@mail.ru");
        order.DateCreated.Should().MatchRegex(DateWithMinutes);
        order.DateSentToCustomer.Should().BeEmpty("заказ ещё не отправлялся клиенту");
        order.JobId.Should().BeNull();

        // Из NotSent домен разрешает переход в NotSent, NeedSignoff и Closed.
        order.Statuses.Select(x => x.Value).Should().BeEquivalentTo(new[]
        {
            OrderStatuses.NotSent, OrderStatuses.NeedSignoff, 5
        });
    }

    [Fact]
    public async Task GetOrderById_UnknownId_ReturnsNotFound()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.GetAsync($"api/orders/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderById_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.GetAsync($"api/orders/{SeedData.SeededOrderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNewOrder_ReturnsEmptyTemplateWithJobTypes()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient();

        //Act
        var response = await client.GetAsync("api/orders/new");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var template = await response.Content.ReadFromJsonAsync<GetNewOrderResponse>(JsonDefaults.Options);

        template!.JobType.Should().Be(0);
        template.CustomerTitle.Should().BeEmpty();
        template.JobDescription.Should().BeEmpty();
        template.JobTypes.Should().HaveCount(4);
        template.JobTypes.Should().ContainSingle(x => x.Value == JobTypes.New && x.Text == "New");
    }
}
