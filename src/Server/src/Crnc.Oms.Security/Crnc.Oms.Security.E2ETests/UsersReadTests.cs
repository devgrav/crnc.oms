using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Security.E2ETests;

// Both GET endpoints below are [AllowAnonymous] in UsersController despite the class-level
// [Authorize] and XML docs saying "Requires admin role" - these tests pin the real behavior
// as it exists today, not the documented intent. Not in scope to "fix" here.
[Collection(SecurityApiCollection.Name)]
public sealed class UsersReadTests
{
    private readonly SecurityApiFixture _fixture;

    public UsersReadTests(SecurityApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUsers_FilteredByAdminRole_ReturnsUsersIncludingSeededAdmin()
    {
        //Act
        var response = await _fixture.Client.GetAsync("api/users?isShortInfo=true&roles=admin");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserShortInfoResponse>>(JsonDefaults.Options);
        users.Should().Contain(u => u.Id == SeedData.AdminUserId && u.Login == SeedData.AdminLogin);
    }

    [Fact]
    public async Task GetUserById_KnownSeededAdminId_ReturnsExpectedUser()
    {
        //Act
        var response = await _fixture.Client.GetAsync($"api/users/{SeedData.AdminUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserItemResponse>(JsonDefaults.Options);
        user.Should().NotBeNull();
        user!.Login.Should().Be(SeedData.AdminLogin);
        user.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserById_UnknownId_ReturnsNotFound()
    {
        //Act
        var response = await _fixture.Client.GetAsync($"api/users/{Guid.NewGuid()}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
