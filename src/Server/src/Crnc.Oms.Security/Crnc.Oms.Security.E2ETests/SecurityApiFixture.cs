using System.Net.Http.Headers;
using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Crnc.Oms.Security.E2ETests;

public sealed class SecurityApiFixture : IAsyncLifetime
{
    private const string MongoNetworkAlias = "security-db";
    private const int MongoContainerPort = 27017;

    private const int ApiContainerPort = 8080;

    private INetwork _network = null!;
    private IContainer _mongoContainer = null!;
    private IContainer _apiContainer = null!;

    public HttpClient Client { get; private set; } = null!;

    public string AdminJwt { get; private set; } = null!;

    public string MainManagerJwt { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder().Build();

        // Plain ContainerBuilder, not the Testcontainers.MongoDb module - that module
        // enables Mongo authorization by default, which this app never speaks (it uses
        // the same unauthenticated setup as docker-compose.yml) and whose auth-aware
        // readiness check hangs indefinitely without WithUsername/WithPassword.
        // Image version must stay in sync with docker-compose.yml's security-db - MongoDB.Driver
        // 3.x requires server wire version >= 9 (MongoDB >= 4.4.0), so anything older than that
        // (e.g. the previous mongo:4.2.3) fails immediately with MongoIncompatibleDriverException.
        _mongoContainer = new ContainerBuilder("mongo:8.3.8")
            .WithNetwork(_network)
            .WithNetworkAliases(MongoNetworkAlias)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(MongoContainerPort))
            .Build();

        await _mongoContainer.StartAsync();

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(FindSecurityServiceDirectory())
            .WithDockerfile("Dockerfile")
            .WithName("crnc-oms-security-api:e2e-tests")
            .WithCleanUp(true)
            .Build();

        await image.CreateAsync();

        _apiContainer = new ContainerBuilder(image)
            .WithNetwork(_network)
            .WithEnvironment("ConnectionStrings:OmsSecurityDb:Server", $"mongodb://{MongoNetworkAlias}")
            .WithEnvironment("Cache:IsUse", "true")
            .WithPortBinding(ApiContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(
                    r => r.ForPort(ApiContainerPort).ForPath("/swagger/index.html"),
                    o => o.WithTimeout(TimeSpan.FromMinutes(3))))
            .Build();

        await _apiContainer.StartAsync();

        var baseAddress = new Uri($"http://{_apiContainer.Hostname}:{_apiContainer.GetMappedPublicPort(ApiContainerPort)}");
        Client = new HttpClient { BaseAddress = baseAddress };

        AdminJwt = await LoginAsync(SeedData.AdminLogin, SeedData.AdminPassword);
        MainManagerJwt = await LoginAsync(SeedData.ShonBeanLogin, SeedData.ShonBeanPassword);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_apiContainer is not null)
            await _apiContainer.DisposeAsync();

        if (_mongoContainer is not null)
            await _mongoContainer.DisposeAsync();

        if (_network is not null)
            await _network.DeleteAsync();
    }

    public HttpClient CreateAuthorizedClient(string jwt)
    {
        var client = new HttpClient { BaseAddress = Client.BaseAddress };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }

    private async Task<string> LoginAsync(string login, string password)
    {
        var response = await Client.PostAsJsonAsync("api/accounts/auth", new AccountRequest(login, password), JsonDefaults.Options);
        response.EnsureSuccessStatusCode();

        var current = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonDefaults.Options);
        return current!.Jwt;
    }

    // Test assembly lands under bin/<Config>/net10.0/, this walks back up to the
    // Security service root so the Docker build context/Dockerfile match what
    // docker-compose.yml already uses - independent of solution-detection heuristics,
    // which would be ambiguous in this repo (multiple .sln files at different levels).
    private static string FindSecurityServiceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Crnc.Oms.Security.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException("Could not locate Crnc.Oms.Security.sln by walking up from the test assembly location.");

        return directory.FullName;
    }
}

[CollectionDefinition(Name)]
public sealed class SecurityApiCollection : ICollectionFixture<SecurityApiFixture>
{
    public const string Name = "SecurityApi";
}
