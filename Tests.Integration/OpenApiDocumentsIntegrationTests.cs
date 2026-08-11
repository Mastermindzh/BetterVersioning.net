using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace BetterVersioning.net.Tests.Integration;

/// <summary>
/// Hosts the Microsoft OpenAPI + Scalar example and asserts that each versioned document
/// contains only its own operations and that deprecated versions carry the deprecation notice.
/// </summary>
public class OpenApiDocumentsIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory = factory;

    [Fact]
    public async Task V37Document_ContainsOnlyThatVersionsOperations()
    {
        var client = factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v37.json"));

        var paths = document.RootElement.GetProperty("paths").EnumerateObject().Select(path => path.Name);
        paths.Should().OnlyContain(path => path.StartsWith("/v37/"));
        document.RootElement.GetProperty("info").GetProperty("version").GetString().Should().Be("37.0");
    }

    [Fact]
    public async Task DeprecatedVersionDocument_CarriesDeprecationNotice()
    {
        var client = factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));

        document.RootElement.GetProperty("info").GetProperty("description").GetString()
            .Should().Contain("deprecated");
    }

    [Fact]
    public async Task Scalar_IsServed()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task MethodOnlyVersionAttributes_FilterOperations()
    {
        var client = factory.CreateClient();

        using var v2 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v2.json"));
        using var v33 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v33.json"));
        using var v34 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v34.json"));
        using var v37 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v37.json"));

        Paths(v2).Should().Contain("/v2/method-only/removed");
        Paths(v37).Should().NotContain("/v37/method-only/removed");
        Paths(v33).Should().NotContain("/v33/method-only/introduced");
        Paths(v34).Should().Contain("/v34/method-only/introduced");
    }

    [Fact]
    public async Task DisjointVersionReplacementActions_DoNotPreventStartup()
    {
        var client = factory.CreateClient();

        using var v2 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v2.json"));
        using var v6 = JsonDocument.Parse(await client.GetStringAsync("/openapi/v6.json"));

        Paths(v2).Should().Contain("/v2/replacement");
        Paths(v6).Should().Contain("/v6/replacement");
    }

    private static IEnumerable<string> Paths(JsonDocument document) =>
        document.RootElement.GetProperty("paths").EnumerateObject().Select(path => path.Name);
}
