using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Crnc.Oms.Security.E2ETests;

[Collection(SecurityApiCollection.Name)]
public sealed class RolesTests
{
    private readonly SecurityApiFixture _fixture;

    public RolesTests(SecurityApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRoles_AsAdmin_ReturnsThreeFixedRoles()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.AdminJwt);

        //Act
        var response = await client.GetAsync("api/roles");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.Content.ReadFromJsonAsync<List<TextValueResponse>>(JsonDefaults.Options);
        roles.Should().HaveCount(3);
        roles.Should().ContainSingle(r => r.Value == SeedData.AdminRoleId && r.Text == "Admin");
        roles.Should().ContainSingle(r => r.Value == SeedData.MainManagerRoleId && r.Text == "Main manager");
        roles.Should().ContainSingle(r => r.Value == SeedData.ManagerRoleId && r.Text == "Manager");
    }

    [Fact]
    public async Task GetRoles_WithoutAuth_ReturnsUnauthorized()
    {
        //Act
        var response = await _fixture.Client.GetAsync("api/roles");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoles_AsNonAdmin_ReturnsForbidden()
    {
        //Arrange
        using var client = _fixture.CreateAuthorizedClient(_fixture.MainManagerJwt);

        //Act
        var response = await client.GetAsync("api/roles");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
