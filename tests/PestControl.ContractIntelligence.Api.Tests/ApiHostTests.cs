using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PestControl.ContractIntelligence.Api.Tests;

public sealed class ApiHostTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly WebApplicationFactory<global::Program> factory;

    public ApiHostTests(WebApplicationFactory<global::Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task LiveHealthEndpointReturnsOkAndCorrelationHeader()
    {
        using var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                "ConnectionStrings:ApplicationDb",
                "Server=(localdb)\\MSSQLLocalDB;Database=PestControlContractIntelligenceTest;Trusted_Connection=True;TrustServerCertificate=True");
        });

        using (var client = testFactory.CreateClient())
        {
            var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Correlation-ID"));
        }
    }
}
