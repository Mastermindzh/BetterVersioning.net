using Microsoft.AspNetCore.Mvc;

using BetterVersioning.Net.Models;
using BetterVersioning.Net.Extensions;

namespace BetterVersioning;

internal class Versions
{
    private readonly IEnumerable<BetterVersion> versions;
    private readonly BetterVersioningOptions options;

    internal Versions(IEnumerable<BetterVersion> versions, BetterVersioningOptions options)
    {
        var allVersions = new List<ApiVersion>();

        foreach (var version in versions)
        {
            allVersions.Add(new ApiVersion(version.MajorVersion, 0));
            if (version.MinorVersions is not null)
            {
                foreach (var minorVersion in version.MinorVersions)
                {
                    allVersions.Add(new ApiVersion(version.MajorVersion, minorVersion));
                }
            }
        }

        this.versions = versions;
        this.options = options;
    }

    /// <summary>
    /// Get a tuple with supported and deprecated versions
    /// </summary>
    /// <param name="from">version to start at</param>
    /// <param name="until">version to end at</param>
    /// <returns></returns>
    internal (IEnumerable<ApiVersion> supported, IEnumerable<ApiVersion> deprecated) GetVersions(ApiVersion? from, ApiVersion? until)
    {
        return (GetSupportedVersions(from, until), GetDeprecatedVersions(from, until));
    }

    /// <summary>
    /// Return all versions that are deprecated
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <returns></returns>
    internal IEnumerable<ApiVersion> GetDeprecatedVersions(ApiVersion? from, ApiVersion? until)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        var versionQuery = versions
            .Where(version => version.Supported == false);

        return ApplyVersionFilters(from, until, ref versionQuery);
    }

    /// <summary>
    /// Returns all versions that are not deprecated
    /// </summary>
    /// <remarks>
    /// Currently just returns the latest version
    /// </remarks>
    /// <returns>array with a single element, the currentVersion</returns>
    internal IEnumerable<ApiVersion> GetSupportedVersions(ApiVersion? from, ApiVersion? until)
    {
        if (until is not null && (from >= until))
        {
            throw new InvalidOperationException($"The from value ({from}) has to be bigger than the until version ({until})");
        }

        var versionQuery = versions
            .Where(version => version.Supported == true);

        return ApplyVersionFilters(from, until, ref versionQuery);

    }

    /// <summary>
    /// Applies the "version >= from" filter and, based on the options, also applies the until
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <param name="versionQuery"></param>
    /// <returns></returns>
    private IEnumerable<ApiVersion> ApplyVersionFilters(ApiVersion? from, ApiVersion? until, ref IEnumerable<BetterVersion> versionQuery)
    {
        var apiVersions = versionQuery.ToApiVersions();

        apiVersions = apiVersions.Where(version => version >= from);

        if (until is not null)
        {
            if (options.UntilInclusive)
            {
                apiVersions = apiVersions.Where(version => version <= until);
            }
            else
            {
                apiVersions = apiVersions.Where(version => version < until);
            }
        }

        return apiVersions;
    }



    /// <summary>
    /// Applies the "version >= from" filter and, based on the options, also applies the until
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <param name="versionQuery"></param>
    /// <returns></returns>
    private IEnumerable<ApiVersion> OldApplyVersionFilters(ApiVersion? from, ApiVersion? until, ref IEnumerable<BetterVersion> versionQuery)
    {
        versionQuery = versionQuery.Where(version => version.MajorVersion >= from?.MajorVersion);

        if (until is not null)
        {
            if (options.UntilInclusive)
            {
                versionQuery = versionQuery.Where(version => version.MajorVersion <= until.MajorVersion);
            }
            else
            {
                versionQuery = versionQuery.Where(version => version.MajorVersion < until.MajorVersion);
            }
        }

        return versionQuery.ToApiVersions();
    }
}
