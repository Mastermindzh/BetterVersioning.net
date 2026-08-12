using Asp.Versioning;
using Asp.Versioning.Conventions;
using BetterVersioning.Net.Extensions;
using BetterVersioning.Net.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BetterVersioning;

public class BetterVersioningConventionBuilder(IEnumerable<BetterVersion> versions, BetterVersioningOptions options) : ApiVersionConventionBuilder
{
    private readonly ApiVersionConventionBuilder apiVersionConventionBuilder = new ApiVersionConventionBuilder();

    internal Versions AllVersions { get; } = new Versions(versions, options);
    private BetterVersioningOptions options { get; } = options;

    public override bool ApplyTo(ControllerModel controllerModel)
    {
        var fromVersion = controllerModel.GetFromVersions();
        var untilVersion = controllerModel.GetUntilVersion();

        ValidateUntilGreaterOrEqualThanFrom(controllerModel, fromVersion, untilVersion);

        if (ShouldConventionApply(controllerModel, fromVersion, untilVersion))
        {
            var controller = apiVersionConventionBuilder.Controller(controllerModel.ControllerType);
            SetControllerApiVersions(controller, fromVersion, untilVersion);
            var actionVersions = SetMethodVersions(controllerModel, fromVersion, untilVersion, controller);
            apiVersionConventionBuilder.ApplyTo(controllerModel);
            if (this.options.DetectDuplicatesAtStartup)
            {
                DetectDuplicates(controllerModel, actionVersions);
            }
        }
        else
        {
            return apiVersionConventionBuilder.ApplyTo(controllerModel);
        }
        return true;
    }
    /// <summary>
    /// Detect duplicate routes caused by BetterVersioning.net
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <exception cref="InvalidOperationException">Thrown when two endpoints share the same HTTP verb and route.</exception>
    private static void DetectDuplicates(
        ControllerModel controllerModel,
        IReadOnlyDictionary<ActionModel, IReadOnlySet<ApiVersion>> actionVersions)
    {
        var endpoints = controllerModel.Actions.SelectMany(action =>
            GetVersionedEndpoints(controllerModel, action, actionVersions[action]));

        if (HasDuplicateEndpoints(endpoints))
        {
            throw new InvalidOperationException($"The ({controllerModel.ControllerName}) controller has duplicate endpoints");
        }
    }

    /// <summary>
    /// Determines whether any two endpoints share an HTTP verb, route template, and API version.
    /// </summary>
    internal static bool HasDuplicateEndpoints(IEnumerable<VersionedEndpoint> endpoints)
    {
        var versionsByEndpoint = new Dictionary<string, List<IReadOnlySet<ApiVersion>>>(StringComparer.OrdinalIgnoreCase);

        foreach (var endpoint in endpoints)
        {
            var key = $"{endpoint.HttpMethod}|{NormalizeRoute(endpoint.RouteTemplate)}";
            if (!versionsByEndpoint.TryGetValue(key, out var existingVersionSets))
            {
                versionsByEndpoint[key] = [endpoint.Versions];
                continue;
            }

            if (existingVersionSets.Any(versions => versions.Overlaps(endpoint.Versions)))
            {
                return true;
            }

            existingVersionSets.Add(endpoint.Versions);
        }

        return false;
    }

    private static IEnumerable<VersionedEndpoint> GetVersionedEndpoints(
        ControllerModel controllerModel,
        ActionModel actionModel,
        IReadOnlySet<ApiVersion> versions)
    {
        var hasControllerRoute = controllerModel.Attributes
            .OfType<IRouteTemplateProvider>()
            .Any(route => route.Template is not null);
        return GetVersionedEndpoints(actionModel.Attributes, hasControllerRoute, versions);
    }

    internal static IEnumerable<VersionedEndpoint> GetVersionedEndpoints(
        IEnumerable<object> actionAttributes,
        bool hasControllerRoute,
        IReadOnlySet<ApiVersion> versions)
    {
        var attributes = actionAttributes.ToArray();
        var separateActionRoutes = attributes
            .OfType<IRouteTemplateProvider>()
            .Where(route => route is not IActionHttpMethodProvider && route.Template is not null)
            .Select(route => route.Template!)
            .ToArray();

        foreach (var httpMethodProvider in attributes.OfType<IActionHttpMethodProvider>())
        {
            var inlineRoute = (httpMethodProvider as IRouteTemplateProvider)?.Template;
            var routes = inlineRoute is not null
                ? [inlineRoute]
                : separateActionRoutes.Length > 0
                    ? separateActionRoutes
                    : hasControllerRoute
                        ? [string.Empty]
                        : [];

            foreach (var route in routes)
            {
                foreach (var httpMethod in httpMethodProvider.HttpMethods)
                {
                    yield return new VersionedEndpoint(httpMethod, route, versions);
                }
            }
        }
    }

    private static string NormalizeRoute(string routeTemplate) =>
        routeTemplate.Trim().TrimStart('~', '/');

    /// <summary>
    /// Check whether this convention should apply.
    /// This convention should apply if we have either a "from" or an "until" attribute on our controller
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <returns></returns>
    private static bool ShouldConventionApply(ControllerModel controllerModel, ApiVersion? from, ApiVersion? until)
    {
        return from is not null ||
            until is not null ||
            controllerModel.Actions.Any(action =>
                action.GetFromVersions() is not null || action.GetUntilVersion() is not null);
    }

    /// <summary>
    /// For every method in a controller add either it's own versions or the controllers versions
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <param name="controllerFrom"></param>
    /// /// <param name="controllerUntil"></param>
    /// <param name="controller"></param>
    private IReadOnlyDictionary<ActionModel, IReadOnlySet<ApiVersion>> SetMethodVersions(ControllerModel controllerModel, ApiVersion? controllerFrom,
        ApiVersion? controllerUntil, IControllerConventionBuilder controller)
    {
        var actionVersions = new Dictionary<ActionModel, IReadOnlySet<ApiVersion>>();

        foreach (var methodModel in controllerModel.Actions)
        {
            var methodFrom = methodModel.GetFromVersions();
            ValidateMethodVersionEqualOrGreaterThanControllerVersion(controllerModel, controllerFrom, methodFrom, methodModel);

            var from = methodFrom ?? controllerFrom;
            var until = methodModel.GetUntilVersion() ?? controllerUntil;
            var (supported, deprecated) = AllVersions.GetVersions(from, until);
            var supportedVersions = supported.ToArray();
            var deprecatedVersions = deprecated.ToArray();
            var method = controller.Action(methodModel.ActionMethod);
            method.HasApiVersions(supportedVersions);
            method.HasDeprecatedApiVersions(deprecatedVersions);
            actionVersions[methodModel] = supportedVersions.Concat(deprecatedVersions).ToHashSet();
        }

        return actionVersions;
    }

    /// <summary>
    /// Set versions on the controller (but not on methods)
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="from"></param>
    /// <param name="until"></param>
    private void SetControllerApiVersions(IControllerConventionBuilder controller,
        ApiVersion? from, ApiVersion? until)
    {
        var (supported, deprecated) = AllVersions.GetVersions(from, until);
        controller.HasApiVersions(supported);
        controller.HasDeprecatedApiVersions(deprecated);
    }

    /// <summary>
    /// Validate that the Controller.Method's from version is greater or equal to the one of it's parent controller
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <param name="controllerFrom"></param>
    /// <param name="methodFrom"></param>
    /// <param name="actionModel"></param>
    private static void ValidateMethodVersionEqualOrGreaterThanControllerVersion(ControllerModel controllerModel, ApiVersion? controllerFrom,
        ApiVersion? methodFrom, ActionModel actionModel)
    {
        if (methodFrom != null && methodFrom < controllerFrom)
        {
            throw new InvalidOperationException($"The methods ({actionModel.ActionName}) from value ({methodFrom}) has to be greater or equal compared to the controller's ({controllerModel.ControllerName}) from version ({controllerFrom}).");
        }
    }

    /// <summary>
    /// Validate that the until value is greater than the from version, allowing equality when
    /// <see cref="BetterVersioningOptions.UntilInclusive"/> is set. An unbounded (<c>null</c>) side is always valid.
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <param name="from"></param>
    /// <param name="until"></param>
    private void ValidateUntilGreaterOrEqualThanFrom(ControllerModel controllerModel, ApiVersion? from,
    ApiVersion? until)
    {
        if (from is null || until is null)
        {
            return;
        }

        if (until < from || (!options.UntilInclusive && until == from))
        {
            throw new InvalidOperationException($"The until value ({until}) has to be greater than{(options.UntilInclusive ? " or equal to" : "")} the from version ({from}) on {controllerModel.ControllerType}");
        }
    }

    internal sealed record VersionedEndpoint(
        string HttpMethod,
        string RouteTemplate,
        IReadOnlySet<ApiVersion> Versions);
}
