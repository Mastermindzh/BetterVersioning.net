using BetterVersioning.Net.Models;
using BetterVersioning.Net.OpenApi;

using FluentAssertions;

namespace BetterVersioning.net.Tests.OpenApi;

public class OpenApiDocumentDeriverTests
{
    [Fact]
    public void Derive_ProducesOneDocumentPerMinorVersion_WithFormattedGroupNames()
    {
        var versions = new[]
        {
            new BetterVersion(1, supported: false),
            new BetterVersion(34, [1, 2]),
            new BetterVersion(37),
        };

        var documents = OpenApiDocumentDeriver.Derive(versions, "'v'VVV");

        documents.Select(document => document.GroupName)
            .Should().BeEquivalentTo("v1", "v34", "v34.1", "v34.2", "v37");
    }

    [Fact]
    public void Derive_SetsApiVersionString()
    {
        var versions = new[] { new BetterVersion(34, [1]) };

        var documents = OpenApiDocumentDeriver.Derive(versions, "'v'VVV");

        documents.Select(document => document.ApiVersion)
            .Should().BeEquivalentTo("34.0", "34.1");
    }

    [Fact]
    public void Derive_MarksDocumentsFromUnsupportedMajorsAsDeprecated()
    {
        var versions = new[]
        {
            new BetterVersion(1, supported: false),
            new BetterVersion(37),
        };

        var documents = OpenApiDocumentDeriver.Derive(versions, "'v'VVV");

        documents.Single(document => document.GroupName == "v1").IsDeprecated.Should().BeTrue();
        documents.Single(document => document.GroupName == "v37").IsDeprecated.Should().BeFalse();
    }
}
