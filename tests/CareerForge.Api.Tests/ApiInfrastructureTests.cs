using System.Net;
using System.Net.Http.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Tests.Infrastructure;

namespace CareerForge.Api.Tests;

public sealed class ApiInfrastructureTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Health_endpoint_is_available()
    {
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Test_database_contains_seeded_catalog()
    {
        var technologies = await client.GetFromJsonAsync<CatalogTechnology[]>("/api/technologies");
        var skills = await client.GetFromJsonAsync<CatalogSkill[]>("/api/skills");

        Assert.NotNull(technologies);
        Assert.NotEmpty(technologies);
        Assert.NotNull(skills);
        Assert.NotEmpty(skills);
    }
}
