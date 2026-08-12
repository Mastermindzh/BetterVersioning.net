namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// The immutable set of versioned OpenAPI documents registered by BetterVersioning.
/// UI integrations consume this catalog so document names are derived only once.
/// </summary>
public sealed class BetterVersioningOpenApiDocumentCatalog
{
    internal BetterVersioningOpenApiDocumentCatalog(
        string groupNameFormat,
        IEnumerable<VersionedOpenApiDocument> documents)
    {
        GroupNameFormat = groupNameFormat;
        Documents = Array.AsReadOnly(documents.ToArray());
    }

    /// <summary>
    /// The API Explorer format used to create the document names.
    /// </summary>
    public string GroupNameFormat { get; }

    /// <summary>
    /// All registered versioned documents.
    /// </summary>
    public IReadOnlyList<VersionedOpenApiDocument> Documents { get; }
}
