using System.Net.Http.Headers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Crnc.Oms.Sales.E2ETests;

public sealed class SalesApiFixture : IAsyncLifetime
{
    private const string PostgresNetworkAlias = "sales-db";
    private const int PostgresContainerPort = 5432;

    private const string RabbitMqNetworkAlias = "message-broker";
    private const int RabbitMqContainerPort = 5672;
    private const int RabbitMqManagementPort = 15672;

    // Заглушка Security поднимается под тем же именем, что настоящий сервис в
    // docker-compose.yml, и на том же порту (8080 — дефолт образа WireMock).
    private const string SecurityStubNetworkAlias = "security-api";
    private const int SecurityStubContainerPort = 8080;

    // Сервис пока на netcoreapp3.1 (базовый образ dotnet/core/aspnet:3.1 слушает 80).
    // После миграции на aspnet:10.0 здесь станет 8080 - см. §9.5 плана миграции.
    private const int ApiContainerPort = 80;

    /// <summary>Очередь, в которую MassTransit складывает команды уведомления
    /// (её же слушает Notification.Gateway, здесь он не поднимается).</summary>
    public const string SendNotificationExchange = "sendNotificationToUser";

    /// <summary>Fanout-exchange события конвертации (его слушает Production).</summary>
    public const string OrderConvertedToJobExchange =
        "Crnc.Oms.Messaging.Contract.Events:OrderConvertedToJobEvent";

    public const string SendNotificationSpyQueue = "e2e-spy-send-notification";
    public const string OrderConvertedToJobSpyQueue = "e2e-spy-order-converted";

    private INetwork _network = null!;
    private IContainer _postgresContainer = null!;
    private IContainer _rabbitMqContainer = null!;
    private IContainer _securityStubContainer = null!;
    private IContainer _apiContainer = null!;

    /// <summary>Клиент без заголовка авторизации — для проверок 401.</summary>
    public HttpClient Client { get; private set; } = null!;

    public WireMockAdmin SecurityStub { get; private set; } = null!;

    public RabbitMqAdmin RabbitMq { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder().Build();

        _postgresContainer = new ContainerBuilder("postgres:18.6")
            .WithNetwork(_network)
            .WithNetworkAliases(PostgresNetworkAlias)
            .WithEnvironment("POSTGRES_PASSWORD", "docker")
            // Postgres поднимает временный сервер во время инициализации и перезапускается,
            // поэтому ждать открытого TCP-порта недостаточно - проверяем pg_isready.
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready", "-U", "postgres"))
            .Build();

        _rabbitMqContainer = new ContainerBuilder("rabbitmq:3-management")
            .WithNetwork(_network)
            .WithNetworkAliases(RabbitMqNetworkAlias)
            .WithPortBinding(RabbitMqManagementPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(RabbitMqContainerPort)
                .UntilHttpRequestIsSucceeded(r => r.ForPort(RabbitMqManagementPort).ForPath("/api/overview")
                    .WithBasicAuthentication("guest", "guest")))
            .Build();

        _securityStubContainer = new ContainerBuilder("wiremock/wiremock:3.13.2")
            .WithNetwork(_network)
            .WithNetworkAliases(SecurityStubNetworkAlias)
            .WithPortBinding(SecurityStubContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(SecurityStubContainerPort).ForPath("/__admin/mappings")))
            .Build();

        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _securityStubContainer.StartAsync());

        SecurityStub = new WireMockAdmin(new Uri(
            $"http://{_securityStubContainer.Hostname}:{_securityStubContainer.GetMappedPublicPort(SecurityStubContainerPort)}"));

        RabbitMq = new RabbitMqAdmin(new Uri(
            $"http://{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(RabbitMqManagementPort)}"));

        // Шпионские очереди готовятся до старта сервиса, чтобы не потерять первое же
        // опубликованное событие.
        await RabbitMq.EnsureSpyQueueAsync(SendNotificationExchange, SendNotificationSpyQueue);
        await RabbitMq.EnsureSpyQueueAsync(OrderConvertedToJobExchange, OrderConvertedToJobSpyQueue);

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(FindSalesServiceDirectory())
            .WithDockerfile("Dockerfile")
            .WithName("crnc-oms-sales-api:e2e-tests")
            .WithCleanUp(true)
            .Build();

        await image.CreateAsync();

        _apiContainer = new ContainerBuilder(image)
            .WithNetwork(_network)
            .WithEnvironment("ConnectionStrings:OmsSalesDb",
                $"Host={PostgresNetworkAlias};Database=crnc_oms_sales_db;Username=postgres;Password=docker")
            .WithEnvironment("IntegrationEndpoints:SecurityServiceEndpoint",
                $"http://{SecurityStubNetworkAlias}:{SecurityStubContainerPort}")
            .WithEnvironment("IntegrationEndpoints:NotificationServiceEndpoint",
                $"http://{SecurityStubNetworkAlias}:{SecurityStubContainerPort}")
            .WithEnvironment("IntegrationEndpoints:MessageBrokerEndpoint",
                $"rabbitmq://{RabbitMqNetworkAlias}")
            // Собственный ключ подписи: набор не должен зависеть от значения в
            // appsettings.json и переживает выравнивание ключей между сервисами.
            .WithEnvironment("Auth:JwtBase64SymmetricKey", SeedData.JwtBase64SymmetricKey)
            .WithPortBinding(ApiContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(
                    r => r.ForPort(ApiContainerPort).ForPath("/swagger/index.html"),
                    o => o.WithTimeout(TimeSpan.FromMinutes(3))))
            .Build();

        await _apiContainer.StartAsync();

        Client = new HttpClient
        {
            BaseAddress = new Uri(
                $"http://{_apiContainer.Hostname}:{_apiContainer.GetMappedPublicPort(ApiContainerPort)}")
        };
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_apiContainer is not null)
            await _apiContainer.DisposeAsync();

        if (_securityStubContainer is not null)
            await _securityStubContainer.DisposeAsync();

        if (_rabbitMqContainer is not null)
            await _rabbitMqContainer.DisposeAsync();

        if (_postgresContainer is not null)
            await _postgresContainer.DisposeAsync();

        if (_network is not null)
            await _network.DeleteAsync();
    }

    public HttpClient CreateAuthorizedClient(string? jwt = null)
    {
        var client = new HttpClient { BaseAddress = Client.BaseAddress };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt ?? TestJwt.ForSeededManager());
        return client;
    }

    // Сборка образа идёт из настоящего контекста сервиса, как это делает docker-compose.
    // Тестовая сборка лежит в bin/<Config>/net10.0/, поднимаемся до .sln сервиса -
    // эвристики поиска solution в этом репозитории неоднозначны (несколько .sln на разных уровнях).
    private static string FindSalesServiceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Crnc.Oms.Sales.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException(
                "Could not locate Crnc.Oms.Sales.sln by walking up from the test assembly location.");

        return directory.FullName;
    }
}

[CollectionDefinition(Name)]
public sealed class SalesApiCollection : ICollectionFixture<SalesApiFixture>
{
    public const string Name = "SalesApi";
}
