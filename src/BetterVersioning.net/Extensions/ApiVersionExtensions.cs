using Asp.Versioning;
using BetterVersioning.Net.Attributes;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BetterVersioning.Net.Extensions;

public static class ApiVersionExtensions
{
    public static ApiVersion? GetFromVersions(this ICommonModel model) =>
         model.Attributes.OfType<From>().Select(a => a.Version).SingleOrDefault();

    public static ApiVersion? GetUntilVersion(this ICommonModel model) =>
        model.Attributes.OfType<Until>().Select(a => a.Version).SingleOrDefault();
}
