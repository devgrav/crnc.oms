using System.Net.Http.Headers;
using Crnc.Oms.Messaging.Contract.Events;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using MassTransit;

namespace Crnc.Oms.Production.E2ETests;

public sealed class ProductionApiFixture : IAsyncLifetime
{
    private const string PostgresNetworkAlias = "production-db";
    private const int PostgresContainerPort = 5432;

    private const string RabbitMqNetworkAlias = "message-broker";
    private const int RabbitMqContainerPort = 5672;
    private const int RabbitMqManagementPort = 15672;

    // Сервис пока на netcoreapp3.1 (базовый образ dotnet/core/aspnet:3.1 слушает 80).
    // После миграции на aspnet:10.0 здесь станет 8080 - см. §8.5 плана миграции.
    private const int ApiContainerPort = 80;

    private static readonly TimeSpan MessageTimeout = TimeSpan.FromSeconds(20);

    private INetwork _network = null!;
    private IContainer _postgresContainer = null!;
    private IContainer _rabbitMqContainer = null!;
    private IContainer _apiContainer = null!;
    private IBusControl _testBus = null!;

    /// <summary>Клиент без заголовка авторизации - для проверок 401.</summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Тестовый бас на MassTransit 8 - решение 8 плана миграции. У Production нет
    /// HTTP-входа для создания работы: единственный путь - OrderConvertedToJobConsumer,
    /// поэтому тест сам публикует событие вместо Sales и сам слушает ответное событие
    /// вместо него же. Топология (fanout-exchange по полному имени типа) поднимается
    /// автоматически самим MassTransit, отдельная работа с RabbitMQ management API
    /// (как RabbitMqAdmin в Crnc.Oms.Sales.E2ETests) здесь не нужна.
    /// </summary>
    public JobCreatedForOrderInbox JobCreatedInbox { get; } = new();

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
            // В отличие от Sales, тестовый процесс сам является клиентом шины (тестовый
            // бас работает на хосте, а не внутри docker-сети), поэтому AMQP-порт нужно
            // пробросить наружу, а не только management-порт.
            .WithPortBinding(RabbitMqContainerPort, true)
            .WithPortBinding(RabbitMqManagementPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(RabbitMqContainerPort)
                .UntilHttpRequestIsSucceeded(r => r.ForPort(RabbitMqManagementPort).ForPath("/api/overview")
                    .WithBasicAuthentication("guest", "guest")))
            .Build();

        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _rabbitMqContainer.StartAsync());

        var rabbitMqHost = _rabbitMqContainer.Hostname;
        var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(RabbitMqContainerPort);

        _testBus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(new Uri($"rabbitmq://{rabbitMqHost}:{rabbitMqPort}/"), h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ReceiveEndpoint("e2e-production-tests-job-created", e =>
            {
                e.Handler<JobCreatedForOrderEvent>(context =>
                {
                    JobCreatedInbox.Add(context.Message);
                    return Task.CompletedTask;
                });
            });
        });

        await _testBus.StartAsync();

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(FindProductionServiceDirectory())
            .WithDockerfile("Dockerfile")
            .WithName("crnc-oms-production-api:e2e-tests")
            .WithCleanUp(true)
            .Build();

        await image.CreateAsync();

        _apiContainer = new ContainerBuilder(image)
            .WithNetwork(_network)
            .WithEnvironment("ConnectionStrings:OmsProductionDb",
                $"Host={PostgresNetworkAlias};Database=crnc_oms_production_db;Username=postgres;Password=docker")
            .WithEnvironment("IntegrationEndpoints:MessageBrokerEndpoint",
                $"rabbitmq://{RabbitMqNetworkAlias}")
            // Собственный ключ подписи: набор не должен зависеть от значения в
            // appsettings.json (там оно уже выровнено с Security) и переживает
            // любую будущую ротацию.
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

        if (_testBus is not null)
            await _testBus.StopAsync();

        if (_apiContainer is not null)
            await _apiContainer.DisposeAsync();

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
            new AuthenticationHeaderValue("Bearer", jwt ?? TestJwt.ForTestUser());
        return client;
    }

    /// <summary>
    /// Публикует событие конвертации заказа - тест играет роль Sales. Возвращает
    /// переданный OrderId для удобства цепочки в тестах.
    /// <para>
    /// jobType/materialSource передаются как есть в конверт - OrderConvertedToJobConsumer
    /// разбирает их через Enum.Parse&lt;JobType&gt;/Enum.Parse&lt;MaterialSource&gt;,
    /// то есть ждёт точное имя члена enum'а (см. EnumMemberNames в TestModels.cs),
    /// а не текст его [Description].
    /// </para>
    /// </summary>
    public async Task<Guid> PublishOrderConvertedToJobAsync(
        Guid orderId,
        string orderNumber,
        string jobType,
        string jobDescription,
        string materialSource,
        string managerFullName,
        string managerLogin)
    {
        await _testBus.Publish<OrderConvertedToJobEvent>(new OrderConvertedToJobEventDto
        {
            OrderId = orderId,
            OrderNumber = orderNumber,
            JobType = jobType,
            JobDescription = jobDescription,
            MaterialSource = materialSource,
            ManagerFullName = managerFullName,
            ManagerLogin = managerLogin
        });

        return orderId;
    }

    public Task<JobCreatedForOrderSnapshot?> WaitForJobCreatedAsync(Guid orderId) =>
        JobCreatedInbox.WaitForAsync(orderId, MessageTimeout);

    // Сборка идёт из настоящего контекста сервиса, как это делает docker-compose.
    // Тестовая сборка лежит в bin/<Config>/net10.0/, поднимаемся до .sln сервиса -
    // эвристики поиска solution в этом репозитории неоднозначны (несколько .sln на разных уровнях).
    private static string FindProductionServiceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Crnc.Oms.Production.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException(
                "Could not locate Crnc.Oms.Production.sln by walking up from the test assembly location.");

        return directory.FullName;
    }
}

[CollectionDefinition(Name)]
public sealed class ProductionApiCollection : ICollectionFixture<ProductionApiFixture>
{
    public const string Name = "ProductionApi";
}
