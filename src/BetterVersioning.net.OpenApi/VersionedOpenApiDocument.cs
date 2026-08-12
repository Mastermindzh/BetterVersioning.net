namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// Describes a single versioned OpenAPI document to register.
/// </summary>
/// <param name="GroupName">The document name / API explorer group name (e.g. <c>v37</c>).</param>
/// <param name="ApiVersion">The formatted API version (e.g. <c>37.0</c>) used for <c>Info.Version</c>.</param>
/// <param name="IsDeprecated">Whether the owning major version is deprecated.</param>
public readonly record struct VersionedOpenApiDocument(string GroupName, string ApiVersion, bool IsDeprecated);
