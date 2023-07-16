using Microsoft.AspNetCore.Mvc;

using BetterVersioning.Net.Models;

namespace BetterVersioning.Net.Extensions;

public static class BetterVersionsExtensions
{

    /// <summary>
    /// Convert the enumerable of BetterVersions to an enumerable of ApiVersions
    /// </summary>
    /// <param name="versions"></param>
    /// <returns></returns>
    public static IEnumerable<ApiVersion> ToApiVersions(this IEnumerable<BetterVersion> versions)
    {
        var returnVersions = new List<ApiVersion>();
        foreach (var version in versions)
        {
            returnVersions.Add(new ApiVersion(version.MajorVersion, 0));
            if (version.MinorVersions is not null)
            {
                foreach (var minorVersion in version.MinorVersions)
                {
                    returnVersions.Add(new ApiVersion(version.MajorVersion, minorVersion));
                }
            }
        }

        return returnVersions;
    }
}
