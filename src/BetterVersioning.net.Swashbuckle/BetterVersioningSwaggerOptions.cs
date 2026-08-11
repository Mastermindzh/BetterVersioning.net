namespace BetterVersioning.Net.Swashbuckle;

/// <summary>
/// Options controlling how BetterVersioning generates one Swagger document per API version.
/// </summary>
public class BetterVersioningSwaggerOptions
{
    /// <summary>
    /// The title shown on every generated Swagger document.
    /// </summary>
    public string Title { get; set; } = "API";

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
