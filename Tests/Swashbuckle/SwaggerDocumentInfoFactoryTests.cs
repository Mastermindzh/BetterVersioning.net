using BetterVersioning.Net.Swashbuckle;

using FluentAssertions;

namespace BetterVersioning.net.Tests.Swashbuckle;

public class SwaggerDocumentInfoFactoryTests
{
    [Fact]
    public void CreateVersionInfo_SetsTitleAndVersion()
    {
        var options = new BetterVersioningSwaggerOptions { Title = "My API" };

        var info = SwaggerDocumentInfoFactory.CreateVersionInfo("37.0", isDeprecated: false, options);

        info.Title.Should().Be("My API");
        info.Version.Should().Be("37.0");
    }

    [Fact]
    public void CreateVersionInfo_SupportedVersion_HasNoDescription()
    {
        var options = new BetterVersioningSwaggerOptions();

        var info = SwaggerDocumentInfoFactory.CreateVersionInfo("37.0", isDeprecated: false, options);

        info.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public void CreateVersionInfo_DeprecatedVersion_AppendsDeprecationNotice()
    {
        var options = new BetterVersioningSwaggerOptions { DeprecationNotice = " gone." };

        var info = SwaggerDocumentInfoFactory.CreateVersionInfo("1.0", isDeprecated: true, options);

        info.Description.Should().Be(" gone.");
    }

    [Fact]
    public void CreateVersionInfo_DeprecatedVersion_WithAnnotationDisabled_HasNoDescription()
    {
        var options = new BetterVersioningSwaggerOptions { AnnotateDeprecatedVersions = false };

        var info = SwaggerDocumentInfoFactory.CreateVersionInfo("1.0", isDeprecated: true, options);

        info.Description.Should().BeNullOrEmpty();
    }
}
