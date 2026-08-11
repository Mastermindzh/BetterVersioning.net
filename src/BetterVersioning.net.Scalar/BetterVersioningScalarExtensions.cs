using BetterVersioning.Net.OpenApi;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Scalar.AspNetCore;

namespace BetterVersioning.Net.Scalar;

/// <summary>
/// Extension methods that render every versioned OpenAPI document in a single Scalar UI.
/// </summary>
public static class BetterVersioningScalarExtensions
{
    /// <summary>
    /// Maps a Scalar API reference using the document catalog registered by
    /// <c>AddBetterVersioningOpenApi</c>.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">Optional additional configuration of the Scalar reference.</param>
    public static IEndpointRouteBuilder MapBetterVersioningScalar(
        this IEndpointRouteBuilder endpoints,
        Action<ScalarOptions>? configure = null)
    {
        var catalog = endpoints.ServiceProvider.GetRequiredService<BetterVersioningOpenApiDocumentCatalog>();
        return MapBetterVersioningScalar(endpoints, catalog.Documents, configure);
    }

    private static IEndpointRouteBuilder MapBetterVersioningScalar(
        IEndpointRouteBuilder endpoints,
        IEnumerable<VersionedOpenApiDocument> documents,
        Action<ScalarOptions>? configure)
    {
        var documentNames = documents
            .Select(document => document.GroupName)
            .ToArray();

        endpoints.MapScalarApiReference(options =>
        {
            options.AddDocuments(documentNames);
            configure?.Invoke(options);
        });

        return endpoints;
    }
}
