using System.Net.Http.Headers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

namespace Crnc.Oms.Notification.E2ETests;

/// <summary>
/// Одна фикстура на весь ограниченный контекст: поднимаются все три сервиса разом.
/// <para>
/// Это отступление от правила «настоящая только своя БД» из конвенции Sales — но
/// отступление мнимое: у Notification нет БД вообще, а Email и Push не соседние
/// контексты, а части того же самого. Разрезав набор на три фикстуры, пришлось бы
/// поднимать RabbitMQ трижды, а главную проверку — цепочку
/// «команда → Gateway → шина → Push → SignalR» — не покрыть вовсе.
/// </para>
/// <para>
/// Настоящие: три сервиса (образы собираются из их собственных Dockerfile) и RabbitMQ.
/// Заглушка: Security → WireMock под тем же сетевым алиасом и портом.
/// Не поднимаются: Sales, Production, SPA, notification-push-client — роль последнего
/// играет SignalR-клиент внутри тестового процесса.
/// </para>
/// </summary>
public sealed class NotificationApiFixture : IAsyncLifetime
{
    private const string RabbitMqNetworkAlias = "message-broker";
    private const int RabbitMqContainerPort = 5672;
    private const int RabbitMqManagementPort = 15672;

    // Заглушка Security поднимается под тем же именем, что настоящий сервис в
    // docker-compose.yml, и на том же порту (8080 — дефолт образа WireMock).
    private const string SecurityStubNetworkAlias = "security-api";
    private const int SecurityStubContainerPort = 8080;

    // Порт у каждого юнита свой, потому что он берётся из его базового образа:
    // dotnet/core/aspnet:3.1 слушает 80, dotnet/aspnet:10.0 — 8080 (§10.4 плана миграции).
    // Юниты мигрируют по одному, поэтому какое-то время значения расходятся.
    private const int EmailContainerPort = 8080;
    private const int PushContainerPort = 8080;
    private const int GatewayContainerPort = 80;

    private const string BrokerEndpoint = "rabbitmq://message-broker";

    private INetwork _network = null!;
    private IContainer _rabbitMqContainer = null!;
    private IContainer _securityStubContainer = null!;
    private IContainer _gatewayContainer = null!;
    private IContainer _emailContainer = null!;
    private IContainer _pushContainer = null!;

    /// <summary>Клиенты без заголовка авторизации — для проверок 401 и анонимных эндпойнтов.</summary>
    public HttpClient GatewayClient { get; private set; } = null!;

    public HttpClient EmailClient { get; private set; } = null!;

    public HttpClient PushClient { get; private set; } = null!;

    public WireMockAdmin SecurityStub { get; private set; } = null!;

    public RabbitMqAdmin RabbitMq { get; private set; } = null!;

    /// <summary>Адрес SignalR-хаба со стороны хоста — для клиента внутри тестового процесса.</summary>
    public string PushHubUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder().Build();

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

        // Брокер обязан быть готов до старта сервисов: MassTransit 6 держит старт хоста,
        // пока шина не подключится, и контейнер зависает на проверке готовности.
        await Task.WhenAll(
            _rabbitMqContainer.StartAsync(),
            _securityStubContainer.StartAsync());

        SecurityStub = new WireMockAdmin(new Uri(
            $"http://{_securityStubContainer.Hostname}:{_securityStubContainer.GetMappedPublicPort(SecurityStubContainerPort)}"));

        RabbitMq = new RabbitMqAdmin(new Uri(
            $"http://{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(RabbitMqManagementPort)}"));

        // Шпионские очереди готовятся до старта сервисов, чтобы не потерять первую же команду.
        await RabbitMq.EnsureSpyQueueAsync(BusNames.SendEmailNotificationToReceiver, BusNames.EmailSpyQueue);
        await RabbitMq.EnsureSpyQueueAsync(BusNames.SendPushNotificationToReceiver, BusNames.PushSpyQueue);

        var contextRoot = FindNotificationContextDirectory();

        var gatewayImage = await BuildImageAsync(contextRoot, "Crnc.Oms.Notification.Gateway",
            "crnc-oms-notification-gateway-api:e2e-tests");
        var emailImage = await BuildImageAsync(contextRoot, "Crnc.Oms.Notification.Email",
            "crnc-oms-notification-email-api:e2e-tests");
        var pushImage = await BuildImageAsync(contextRoot, "Crnc.Oms.Notification.Push",
            "crnc-oms-notification-push-api:e2e-tests");

        _emailContainer = new ContainerBuilder(emailImage)
            .WithNetwork(_network)
            .WithNetworkAliases("notification-email-api")
            .WithEnvironment("IntegrationEndpoints:MessageBrokerEndpoint", BrokerEndpoint)
            .WithEnvironment("Auth:JwtBase64SymmetricKey", SeedData.JwtBase64SymmetricKey)
            .WithPortBinding(EmailContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                r => r.ForPort(EmailContainerPort).ForPath("/swagger/index.html")))
            .Build();

        _pushContainer = new ContainerBuilder(pushImage)
            .WithNetwork(_network)
            .WithNetworkAliases("notification-push-api")
            .WithEnvironment("IntegrationEndpoints:MessageBrokerEndpoint", BrokerEndpoint)
            .WithEnvironment("IntegrationEndpoints:UiEndpoint", "http://localhost:8092")
            .WithEnvironment("Auth:JwtBase64SymmetricKey", SeedData.JwtBase64SymmetricKey)
            .WithPortBinding(PushContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                r => r.ForPort(PushContainerPort).ForPath("/swagger/index.html")))
            .Build();

        _gatewayContainer = new ContainerBuilder(gatewayImage)
            .WithNetwork(_network)
            .WithNetworkAliases("notification-gateway-api")
            .WithEnvironment("IntegrationEndpoints:MessageBrokerEndpoint", BrokerEndpoint)
            .WithEnvironment("IntegrationEndpoints:SecurityServiceEndpoint",
                $"http://{SecurityStubNetworkAlias}:{SecurityStubContainerPort}")
            .WithEnvironment("IntegrationEndpoints:EmailNotificationServiceEndpoint",
                "http://notification-email-api")
            .WithEnvironment("IntegrationEndpoints:PushNotificationServiceEndpoint",
                "http://notification-push-api")
            .WithEnvironment("Auth:JwtBase64SymmetricKey", SeedData.JwtBase64SymmetricKey)
            .WithPortBinding(GatewayContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                r => r.ForPort(GatewayContainerPort).ForPath("/swagger/index.html")))
            .Build();

        await Task.WhenAll(
            _emailContainer.StartAsync(),
            _pushContainer.StartAsync(),
            _gatewayContainer.StartAsync());

        GatewayClient = CreateClient(_gatewayContainer, GatewayContainerPort);
        EmailClient = CreateClient(_emailContainer, EmailContainerPort);
        PushClient = CreateClient(_pushContainer, PushContainerPort);

        PushHubUrl =
            $"http://{_pushContainer.Hostname}:{_pushContainer.GetMappedPublicPort(PushContainerPort)}/hubs/push";
    }

    public async Task DisposeAsync()
    {
        GatewayClient?.Dispose();
        EmailClient?.Dispose();
        PushClient?.Dispose();

        if (_gatewayContainer is not null)
            await _gatewayContainer.DisposeAsync();

        if (_pushContainer is not null)
            await _pushContainer.DisposeAsync();

        if (_emailContainer is not null)
            await _emailContainer.DisposeAsync();

        if (_securityStubContainer is not null)
            await _securityStubContainer.DisposeAsync();

        if (_rabbitMqContainer is not null)
            await _rabbitMqContainer.DisposeAsync();

        if (_network is not null)
            await _network.DeleteAsync();
    }

    public HttpClient CreateAuthorizedGatewayClient(string? jwt = null)
    {
        var client = new HttpClient { BaseAddress = GatewayClient.BaseAddress };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt ?? TestJwt.ForReceiver());
        return client;
    }

    /// <summary>
    /// Логи Email-сервиса. Единственный наблюдаемый снаружи результат его работы —
    /// строка в логе: настоящей отправки письма в системе нет, EmailGateway пишет в лог.
    /// </summary>
    public async Task<string> GetEmailServiceLogsAsync()
    {
        var (stdout, stderr) = await _emailContainer.GetLogsAsync(timestampsEnabled: false);
        return stdout + stderr;
    }

    private static HttpClient CreateClient(IContainer container, int containerPort) => new()
    {
        BaseAddress = new Uri($"http://{container.Hostname}:{container.GetMappedPublicPort(containerPort)}")
    };

    private static async Task<IImage> BuildImageAsync(string contextRoot, string unitFolder, string tag)
    {
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(Path.Combine(contextRoot, unitFolder))
            .WithDockerfile("Dockerfile")
            .WithName(tag)
            .WithCleanUp(false)
            .Build();

        await image.CreateAsync();

        return image;
    }

    // Сборка образов идёт из настоящих контекстов юнитов, как это делает docker-compose.
    // Тестовая сборка лежит в bin/<Config>/net10.0/, поднимаемся до зонтичного .sln контекста —
    // эвристики поиска solution в этом репозитории неоднозначны (несколько .sln на разных уровнях).
    private static string FindNotificationContextDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "Crnc.Oms.Notification.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException(
                "Could not locate Crnc.Oms.Notification.sln by walking up from the test assembly location.");

        return directory.FullName;
    }
}

[CollectionDefinition(Name)]
public sealed class NotificationApiCollection : ICollectionFixture<NotificationApiFixture>
{
    public const string Name = "notification-api";
}
