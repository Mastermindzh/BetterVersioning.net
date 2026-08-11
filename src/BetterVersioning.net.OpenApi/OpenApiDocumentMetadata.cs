using Microsoft.OpenApi;

namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// Pure, host-free application of BetterVersioning's per-version metadata onto an
/// OpenAPI document's <see cref="OpenApiInfo"/> (title, version and deprecation notice).
/// </summary>
internal static class OpenApiDocumentMetadata
{
    internal static void Apply(
        OpenApiInfo info,
        string apiVersion,
        bool isDeprecated,
        BetterVersioningOpenApiOptions options)
    {
        info.Title = options.Title;
        info.Version = apiVersion;

        if (isDeprecated && options.AnnotateDeprecatedVersions)
        {
            info.Description += options.DeprecationNotice;
        }
    }
}
