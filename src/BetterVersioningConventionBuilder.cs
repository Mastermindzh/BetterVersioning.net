using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;

using BetterVersioning.Net.Extensions;
using BetterVersioning.Net.Models;

namespace BetterVersioning;

public class BetterVersioningConventionBuilder : ApiVersionConventionBuilder
{
    private readonly ApiVersionConventionBuilder apiVersionConventionBuilder;

    internal Versions AllVersions { get; }
    private BetterVersioningOptions options { get; }

    public BetterVersioningConventionBuilder(IEnumerable<BetterVersion> versions, BetterVersioningOptions options)
    {
        this.options = options;
        AllVersions = new Versions(versions, options);
        apiVersionConventionBuilder = new ApiVersionConventionBuilder();
    }

    public override bool ApplyTo(ControllerModel controllerModel)
    {
        var fromVersion = controllerModel.GetFromVersions();
        var untilVersion = controllerModel.GetUntilVersion();

        ValidateUntilGreaterOrEqualThanFrom(controllerModel, fromVersion, untilVersion);

        if (ShouldConventionApply(fromVersion, untilVersion))
        {
            var controller = apiVersionConventionBuilder.Controller(controllerModel.ControllerType);
            SetControllerApiVersions(controller, fromVersion, untilVersion);
            SetMethodVersions(controllerModel, fromVersion, untilVersion, controller);
            apiVersionConventionBuilder.ApplyTo(controllerModel);
            if (this.options.DetectDuplicatesAtStartup)
            {
                DetectDuplicates(controllerModel);
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
    /// <exception cref="InvalidOperationException"></exception> 
    /// <summary></summary>
    /// <param name="controllerModel"></param>
    private static void DetectDuplicates(ControllerModel controllerModel)
    {
        // find and group all http methods by their template string (a.k.a route)
        var groupedHttpMethods = controllerModel.Actions.SelectMany(static action =>
            action.Attributes.Where(
                attribute => attribute.GetType().IsSubclassOf(typeof(HttpMethodAttribute)))
                .Cast<HttpMethodAttribute>()
                .ToList()
        ).GroupBy(methodAttribute => methodAttribute.Template);

        // Check whether duplicate strings are found, if not also check whether both `null` and "" are found
        var duplicatesFound =
            groupedHttpMethods.Any(group => group.Count() > 1) ||
            groupedHttpMethods.Count(group => string.IsNullOrEmpty(group.Key)) > 1;

        if (duplicatesFound)
        {
            throw new InvalidOperationException($"The ({controllerModel.ControllerName}) controller has duplicate endpoints");
        }
    }

    /// <summary>
    /// Check whether this convention should apply.
    /// This convention should apply if we have either a "from" or an "until" attribute on our controller
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <returns></returns>
    private bool ShouldConventionApply(ApiVersion? from, ApiVersion? until)
    {
        return from is not null || until is not null;
    }

    /// <summary>
    /// For every method in a controller add either it's own versions or the controllers versions
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <param name="controllerFrom"></param>
    /// /// <param name="controllerUntil"></param>
    /// <param name="controller"></param>
    private void SetMethodVersions(ControllerModel controllerModel, ApiVersion? controllerFrom,
        ApiVersion? controllerUntil, IControllerConventionBuilder controller)
    {
        foreach (var methodModel in controllerModel.Actions)
        {
            var methodFrom = methodModel.GetFromVersions();
            ValidateMethodVersionEqualOrGreaterThanControllerVersion(controllerModel, controllerFrom, methodFrom, methodModel);

            var from = methodFrom ?? controllerFrom;
            var until = methodModel.GetUntilVersion() ?? controllerUntil;
            var (supported, deprecated) = AllVersions.GetVersions(from, until);
            var method = controller.Action(methodModel.ActionMethod);
            method.HasApiVersions(supported);
            method.HasDeprecatedApiVersions(deprecated);
        }
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
    /// Validate that the until value passed is actually bigger or equal compared to the from version
    /// </summary>
    /// <param name="controllerModel"></param>
    /// <param name="from"></param>
    /// <param name="until"></param>
    private static void ValidateUntilGreaterOrEqualThanFrom(ControllerModel controllerModel, ApiVersion? from,
    ApiVersion? until)
    {
        if (until is not null && until <= from)
        {
            throw new InvalidOperationException($"The from value ({from}) has to be smaller or equal to the until version ({until}) on {controllerModel.ControllerType}");
        }
    }
}
