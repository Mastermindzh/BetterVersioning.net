using Microsoft.AspNetCore.Mvc;

using BetterVersioning.Net.Models;

namespace BetterVersioning;

internal class Versions
{
    private readonly IEnumerable<ApiVersion> supportedVersions;
    private readonly IEnumerable<ApiVersion> deprecatedVersions;
    private readonly BetterVersioningOptions options;

    internal Versions(IEnumerable<BetterVersion> versions, BetterVersioningOptions options)
    {
        var supported = new List<ApiVersion>();
        var deprecated = new List<ApiVersion>();
        var majorVersions = new HashSet<ushort>();

        foreach (var version in versions)
        {
            if (!majorVersions.Add(version.MajorVersion))
            {
                throw new ArgumentException("Versions contains duplicate major versions", nameof(versions));
            }

            var allVersions = version.Supported ? supported : deprecated;
            allVersions.AddRange(version.MinorVersions.Select(minorVersion => new ApiVersion(version.MajorVersion, minorVersion)));
        }

        supportedVersions = supported;
        deprecatedVersions = deprecated;
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
        ArgumentNullException.ThrowIfNull(from);

        return (GetSupportedVersions(from, until), GetDeprecatedVersions(from, until));
    }

    /// <summary>
    /// Return all versions that are deprecated
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <returns></returns>
    private IEnumerable<ApiVersion> GetDeprecatedVersions(ApiVersion from, ApiVersion? until)
    {
        return ApplyVersionFilters(from, until, deprecatedVersions);
    }

    /// <summary>
    /// Returns all versions that are not deprecated
    /// </summary>
    /// <remarks>
    /// Currently just returns the latest version
    /// </remarks>
    /// <returns>array with a single element, the currentVersion</returns>
    private IEnumerable<ApiVersion> GetSupportedVersions(ApiVersion from, ApiVersion? until)
    {
        if (until is not null)
        {
            if (from > until)
            {
                throw new InvalidOperationException($"The from value ({from}) has to be smaller than or equal to the until version ({until})");
            }
            if (!options.UntilInclusive && from == until)
            {
                throw new InvalidOperationException($"The from value ({from}) can only be equal to the until version ({until}) if the `UntilInclusive` option is set.");
            }
        }

        return ApplyVersionFilters(from, until, supportedVersions);

    }

    /// <summary>
    /// Applies the "version >= from" filter and, based on the options, also applies the until
    /// </summary>
    /// <param name="from"></param>
    /// <param name="until"></param>
    /// <param name="apiVersions"></param>
    /// <returns></returns>
    private IEnumerable<ApiVersion> ApplyVersionFilters(ApiVersion from, ApiVersion? until, IEnumerable<ApiVersion> apiVersions)
    {
        apiVersions = apiVersions.Where(version => version >= from);

        if (until is not null)
        {
            apiVersions = options.UntilInclusive
                ? apiVersions.Where(version => version <= until)
                : apiVersions.Where(version => version < until);
        }

        return apiVersions;
    }
}
