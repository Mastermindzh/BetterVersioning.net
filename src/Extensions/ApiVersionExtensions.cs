using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

using BetterVersioning.Net.Attributes;

namespace BetterVersioning.Net.Extensions;

public static class ApiVersionExtensions
{
    public static ApiVersion? GetFromVersions(this ICommonModel model) =>
         model.Attributes.OfType<From>().Select(a => a.Version).SingleOrDefault();

    public static ApiVersion? GetUntilVersion(this ICommonModel model) =>
        model.Attributes.OfType<Until>().Select(a => a.Version).SingleOrDefault();
}
