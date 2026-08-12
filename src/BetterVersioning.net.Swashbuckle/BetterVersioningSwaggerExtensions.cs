using Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace BetterVersioning.Net.Swashbuckle;

/// <summary>
/// Extension methods that wire BetterVersioning's per-version Swagger documents and UI.
/// </summary>
public static class BetterVersioningSwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation with one document per configured API version. Deprecated
    /// versions carry a deprecation notice in their document description.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration of <see cref="BetterVersioningSwaggerOptions"/>.</param>
    public static IServiceCollection AddBetterVersioningSwagger(
        this IServiceCollection services,
        Action<BetterVersioningSwaggerOptions>? configure = null)
    {
        var options = new BetterVersioningSwaggerOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureBetterVersioningSwaggerGenOptions>();

        return services;
    }

    /// <summary>
    /// Serves the generated documents and a Swagger UI with one endpoint per API version.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configure">Optional additional configuration of the Swagger UI.</param>
    public static IApplicationBuilder UseBetterVersioningSwaggerUI(
        this IApplicationBuilder app,
        Action<SwaggerUIOptions>? configure = null)
    {
        var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(uiOptions =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                uiOptions.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }

            configure?.Invoke(uiOptions);
        });

        return app;
    }
}
