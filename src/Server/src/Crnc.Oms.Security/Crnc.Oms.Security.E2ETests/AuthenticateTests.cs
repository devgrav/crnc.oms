using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Security.E2ETests;

[Collection(SecurityApiCollection.Name)]
public sealed class AuthenticateTests
{
    private readonly SecurityApiFixture _fixture;

    public AuthenticateTests(SecurityApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Authenticate_ValidAdminCredentials_ReturnsJwtAndUserInfo()
    {
        //Arrange
        var request = new AccountRequest(SeedData.AdminLogin, SeedData.AdminPassword);

        //Act
        var response = await _fixture.Client.PostAsJsonAsync("api/accounts/auth", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonDefaults.Options);
        body.Should().NotBeNull();
        body!.Login.Should().Be(SeedData.AdminLogin);
        body.Role.Should().Be("Admin");
        body.Jwt.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Authenticate_WrongPassword_ReturnsBadRequest()
    {
        //Arrange
        var request = new AccountRequest(SeedData.AdminLogin, "definitely-wrong-password");

        //Act
        var response = await _fixture.Client.PostAsJsonAsync("api/accounts/auth", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Not valid login or password");
    }

    [Fact]
    public async Task Authenticate_UnknownLogin_ReturnsBadRequest()
    {
        //Arrange
        var request = new AccountRequest($"unknown_{Guid.NewGuid():N}", "111111");

        //Act
        var response = await _fixture.Client.PostAsJsonAsync("api/accounts/auth", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Not valid login or password");
    }

    [Fact]
    public async Task Authenticate_EmptyBody_ReturnsBadRequest()
    {
        //Arrange
        var request = new AccountRequest(string.Empty, string.Empty);

        //Act
        var response = await _fixture.Client.PostAsJsonAsync("api/accounts/auth", request, JsonDefaults.Options);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
