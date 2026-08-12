using Asp.Versioning.ApiExplorer;

using BetterVersioning.Net.OpenApi;

using FluentAssertions;

namespace BetterVersioning.net.Tests.OpenApi;

public class BetterVersioningOpenApiExtensionsTests
{
    [Fact]
    public void ValidateGroupNameFormat_WithMatchingFormats_DoesNotThrow()
    {
        var catalog = Catalog("'v'VVV");
        var explorerOptions = new ApiExplorerOptions { GroupNameFormat = "'v'VVV" };

        var validate = () => BetterVersioningOpenApiExtensions.ValidateGroupNameFormat(catalog, explorerOptions);

        validate.Should().NotThrow();
    }

    [Fact]
    public void ValidateGroupNameFormat_WithDifferentFormats_Throws()
    {
        var catalog = Catalog("'v'VVV");
        var explorerOptions = new ApiExplorerOptions { GroupNameFormat = "'version-'VVV" };

        var validate = () => BetterVersioningOpenApiExtensions.ValidateGroupNameFormat(catalog, explorerOptions);

        validate.Should().Throw<InvalidOperationException>()
            .WithMessage("*group-name format*Configure both with the same format*");
    }

    private static BetterVersioningOpenApiDocumentCatalog Catalog(string groupNameFormat) =>
        new(groupNameFormat, [new VersionedOpenApiDocument("v1", "1.0", false)]);
}
