namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// Options controlling how BetterVersioning registers one Microsoft OpenAPI document per API version.
/// </summary>
public class BetterVersioningOpenApiOptions
{
    /// <summary>
    /// The title applied to every generated OpenAPI document.
    /// </summary>
    public string Title { get; set; } = "API";

    /// <summary>
    /// The format used to derive each document's name from its API version, matching
    /// <c>ApiExplorerOptions.GroupNameFormat</c> so document names align with routing.
    /// </summary>
    public string GroupNameFormat { get; set; } = "'v'VVV";

    /// <summary>
    /// Text appended to the description of a deprecated version's document when
    /// <see cref="AnnotateDeprecatedVersions"/> is enabled.
    /// </summary>
    public string DeprecationNotice { get; set; } =
        " This API version has been deprecated. Please use one of the newer API versions available.";

    /// <summary>
    /// Whether to append <see cref="DeprecationNotice"/> to deprecated versions' documents.
    /// </summary>
    /// <value><c>true</c> by default.</value>
    public bool AnnotateDeprecatedVersions { get; set; } = true;
}
