using System.Globalization;

using Asp.Versioning;

using BetterVersioning.Net.Models;

namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// Pure, host-free derivation of the versioned OpenAPI documents to register from the
/// configured <see cref="BetterVersion"/> list. One document is produced per distinct
/// API version (major + each minor), with the group name formatted to match routing.
/// </summary>
internal static class OpenApiDocumentDeriver
{
    internal static IReadOnlyList<VersionedOpenApiDocument> Derive(
        IEnumerable<BetterVersion> versions,
        string groupNameFormat)
    {
        var documents = new List<VersionedOpenApiDocument>();

        foreach (var version in versions)
        {
            foreach (var minor in version.MinorVersions)
            {
                var apiVersion = new ApiVersion(version.MajorVersion, minor);
                documents.Add(new VersionedOpenApiDocument(
                    apiVersion.ToString(groupNameFormat, CultureInfo.InvariantCulture),
                    apiVersion.ToString(),
                    !version.Supported));
            }
        }

        return documents;
    }
}
