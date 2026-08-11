using Microsoft.OpenApi;

namespace BetterVersioning.Net.Swashbuckle;

/// <summary>
/// Pure, host-free construction of the per-version <see cref="OpenApiInfo"/> for a Swagger document.
/// </summary>
internal static class SwaggerDocumentInfoFactory
{
    /// <summary>
    /// Builds the <see cref="OpenApiInfo"/> for a single API version's Swagger document,
    /// appending the deprecation notice when the version is deprecated and annotation is enabled.
    /// </summary>
    internal static OpenApiInfo CreateVersionInfo(
        string apiVersion,
        bool isDeprecated,
        BetterVersioningSwaggerOptions options)
    {
        var info = new OpenApiInfo
        {
            Title = options.Title,
            Version = apiVersion,
        };

        if (isDeprecated && options.AnnotateDeprecatedVersions)
        {
            info.Description += options.DeprecationNotice;
        }

        return info;
    }
}
