using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Crnc.Oms.Security.E2ETests;

[Collection(SecurityApiCollection.Name)]
public sealed class UsersWriteTests
{
    private readonly SecurityApiFixture _fixture;

    public UsersWriteTests(SecurityApiFixture fixture)
    {
        _fixture = fixture;
    }

    private static SaveUserRequest NewUserRequest(string? loginOverride = null) =>
        new(
            FirstName: "E2E",
            LastName: "Test",
            Email: $"{Guid.NewGuid():N}@e2e.test",
            Login: loginOverride ?? $"e2e_{Guid.NewGuid():N}",
            Password: "111111",
            Phone: null,
            RoleId: SeedData.ManagerRoleId);

    [Fact]
    public async Task CreateUser_ValidPayload_ReturnsNewUserId()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);
        var request = NewUserRequest();

        //Act
        var response = await client.PostAsJsonAsync("api/users", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = await response.Content.ReadFromJsonAsync<Guid>(JsonDefaults.Options);
        newId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"api/users/{newId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys()
    {
        //Arrange - regression test for the System.Text.Json DictionaryKeyPolicy risk
        //flagged in docs/migrations/security-net10-migration-plan.md: ModelState errors
        //must stay camelCase ("firstName"), not regress to PascalCase ("FirstName").
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);
        var request = NewUserRequest() with { FirstName = null! };

        //Act
        var response = await client.PostAsJsonAsync("api/users", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonDefaults.Options);
        json.TryGetProperty("firstName", out _).Should().BeTrue("validation error keys should stay camelCase");
        json.TryGetProperty("FirstName", out _).Should().BeFalse("validation error keys should not regress to PascalCase");
    }

    [Fact]
    public async Task CreateUser_DuplicateLoginDifferentCase_ReturnsBadRequest()
    {
        //Arrange - regression test for the MongoDB.Driver LINQ2->LINQ3 risk flagged in
        //docs/migrations/security-net10-migration-plan.md: UserQueries.IsExisted's
        //x.Login.ToLower() == entity.Login.ToLower() predicate must still catch a
        //case-varied duplicate login after the driver upgrade.
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);
        var login = $"e2e_{Guid.NewGuid():N}";
        var firstResponse = await client.PostAsJsonAsync("api/users", NewUserRequest(login), JsonDefaults.Options);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //Act
        var response = await client.PostAsJsonAsync("api/users", NewUserRequest(login.ToUpperInvariant()), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("User has already existed");
    }

    [Fact]
    public async Task CreateUser_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.PostAsJsonAsync("api/users", NewUserRequest(), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_AsNonAdmin_ReturnsForbidden()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.MainManagerJwt);

        //Act
        var response = await client.PostAsJsonAsync("api/users", NewUserRequest(), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_ExistingUser_UpdatesFields()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);
        var createResponse = await client.PostAsJsonAsync("api/users", NewUserRequest(), JsonDefaults.Options);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>(JsonDefaults.Options);
        var updateRequest = NewUserRequest() with { FirstName = "Updated" };

        //Act
        var updateResponse = await client.PutAsJsonAsync($"api/users/{id}", updateRequest, JsonDefaults.Options);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await client.GetAsync($"api/users/{id}");
        var user = await getResponse.Content.ReadFromJsonAsync<UserItemResponse>(JsonDefaults.Options);
        user!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateUser_UnknownId_ReturnsNotFound()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);

        //Act
        var response = await client.PutAsJsonAsync($"api/users/{Guid.NewGuid()}", NewUserRequest(), JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_RemovesUser()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);
        var createResponse = await client.PostAsJsonAsync("api/users", NewUserRequest(), JsonDefaults.Options);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>(JsonDefaults.Options);

        //Act
        var deleteResponse = await client.DeleteAsync($"api/users/{id}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await client.GetAsync($"api/users/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
