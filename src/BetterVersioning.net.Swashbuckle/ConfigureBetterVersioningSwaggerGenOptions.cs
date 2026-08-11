using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace BetterVersioning.Net.Swashbuckle;

/// <summary>
/// Registers one <c>SwaggerDoc</c> per discovered API version, applying the deprecation
/// notice for deprecated versions. Relies on the versioned API explorer to enumerate versions.
/// </summary>
internal sealed class ConfigureBetterVersioningSwaggerGenOptions(
    IApiVersionDescriptionProvider provider,
    BetterVersioningSwaggerOptions options)
        : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider = provider;
    private readonly BetterVersioningSwaggerOptions options = options;

    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            swaggerGenOptions.SwaggerDoc(
                description.GroupName,
                SwaggerDocumentInfoFactory.CreateVersionInfo(
                    description.ApiVersion.ToString(),
                    description.IsDeprecated,
                    options));
        }
    }
}
