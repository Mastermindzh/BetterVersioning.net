using BetterVersioning.Net.OpenApi;

using FluentAssertions;

using Microsoft.OpenApi;

namespace BetterVersioning.net.Tests.OpenApi;

public class OpenApiDocumentMetadataTests
{
    [Fact]
    public void Apply_SetsTitleAndVersion()
    {
        var info = new OpenApiInfo();
        var options = new BetterVersioningOpenApiOptions { Title = "My API" };

        OpenApiDocumentMetadata.Apply(info, "37.0", isDeprecated: false, options);

        info.Title.Should().Be("My API");
        info.Version.Should().Be("37.0");
        info.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Apply_DeprecatedVersion_AppendsNotice()
    {
        var info = new OpenApiInfo();
        var options = new BetterVersioningOpenApiOptions { DeprecationNotice = " gone." };

        OpenApiDocumentMetadata.Apply(info, "1.0", isDeprecated: true, options);

        info.Description.Should().Be(" gone.");
    }

    [Fact]
    public void Apply_DeprecatedVersion_WithAnnotationDisabled_HasNoNotice()
    {
        var info = new OpenApiInfo();
        var options = new BetterVersioningOpenApiOptions { AnnotateDeprecatedVersions = false };

        OpenApiDocumentMetadata.Apply(info, "1.0", isDeprecated: true, options);

        info.Description.Should().BeNullOrEmpty();
    }
}
