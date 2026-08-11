using BetterVersioning.Net.Models;

using Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BetterVersioning.Net.OpenApi;

/// <summary>
/// Extension methods that register and map one Microsoft OpenAPI document per API version.
/// </summary>
public static class BetterVersioningOpenApiExtensions
{
    /// <summary>
    /// Registers one OpenAPI document per configured API version, filtered by group name,
    /// with per-document title, version and deprecation metadata applied via a document transformer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="versions">The configured versions, the single source of truth for the documents.</param>
    /// <param name="configure">Optional configuration of <see cref="BetterVersioningOpenApiOptions"/>.</param>
    public static IServiceCollection AddBetterVersioningOpenApi(
        this IServiceCollection services,
        IEnumerable<BetterVersion> versions,
        Action<BetterVersioningOpenApiOptions>? configure = null)
    {
        var options = new BetterVersioningOpenApiOptions();
        configure?.Invoke(options);

        var documents = OpenApiDocumentDeriver.Derive(versions, options.GroupNameFormat);
        services.AddSingleton(new BetterVersioningOpenApiDocumentCatalog(options.GroupNameFormat, documents));

        foreach (var document in documents)
        {
            services.AddOpenApi(document.GroupName, openApiOptions =>
            {
                openApiOptions.ShouldInclude = api => api.GroupName == document.GroupName;
                openApiOptions.AddDocumentTransformer((openApiDocument, _, _) =>
                {
                    OpenApiDocumentMetadata.Apply(
                        openApiDocument.Info,
                        document.ApiVersion,
                        document.IsDeprecated,
                        options);
                    return Task.CompletedTask;
                });
            });
        }

        return services;
    }

    /// <summary>
    /// Returns the exact document catalog registered by <see cref="AddBetterVersioningOpenApi"/>.
    /// </summary>
    /// <param name="services">The application's service provider.</param>
    public static IReadOnlyList<VersionedOpenApiDocument> GetVersionedDocuments(this IServiceProvider services) =>
        services.GetRequiredService<BetterVersioningOpenApiDocumentCatalog>().Documents;

    /// <summary>
    /// Maps <c>/openapi/{documentName}.json</c> for every registered versioned document.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapBetterVersioningOpenApi(this IEndpointRouteBuilder endpoints)
    {
        var catalog = endpoints.ServiceProvider.GetRequiredService<BetterVersioningOpenApiDocumentCatalog>();
        var explorerOptions = endpoints.ServiceProvider.GetService<IOptions<ApiExplorerOptions>>()?.Value
            ?? throw new InvalidOperationException(
                "API version explorer services are required. Call AddApiVersioning().AddApiExplorer() before AddBetterVersioningOpenApi().");

        ValidateGroupNameFormat(catalog, explorerOptions);

        endpoints.MapOpenApi();
        return endpoints;
    }

    internal static void ValidateGroupNameFormat(
        BetterVersioningOpenApiDocumentCatalog catalog,
        ApiExplorerOptions explorerOptions)
    {
        if (!string.Equals(
            catalog.GroupNameFormat,
            explorerOptions.GroupNameFormat,
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"BetterVersioning OpenAPI uses group-name format '{catalog.GroupNameFormat}', " +
                $"but API Explorer uses '{explorerOptions.GroupNameFormat}'. Configure both with the same format.");
        }
    }
}
