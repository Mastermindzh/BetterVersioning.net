using Asp.Versioning;

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

            var target = version.Supported ? supported : deprecated;
            target.AddRange(version.MinorVersions.Select(minorVersion => new ApiVersion(version.MajorVersion, minorVersion)));
        }

        supportedVersions = supported;
        deprecatedVersions = deprecated;
        this.options = options;
    }

    /// <summary>
    /// Get a tuple with supported and deprecated versions for the given range.
    /// </summary>
    /// <param name="from">version to start at (inclusive), or <c>null</c> for no lower bound</param>
    /// <param name="until">version to end at, or <c>null</c> for no upper bound</param>
    internal (IEnumerable<ApiVersion> supported, IEnumerable<ApiVersion> deprecated) GetVersions(ApiVersion? from, ApiVersion? until)
    {
        ValidateRange(from, until);

        return (
            ApplyVersionFilters(from, until, supportedVersions),
            ApplyVersionFilters(from, until, deprecatedVersions)
        );
    }

    /// <summary>
    /// Validates the range is coherent, honouring the <see cref="BetterVersioningOptions.UntilInclusive"/> option.
    /// An unbounded (<c>null</c>) side is always valid.
    /// </summary>
    private void ValidateRange(ApiVersion? from, ApiVersion? until)
    {
        if (from is null || until is null)
        {
            return;
        }

        if (from > until)
        {
            throw new InvalidOperationException($"The from value ({from}) has to be smaller than or equal to the until version ({until})");
        }

        if (!options.UntilInclusive && from == until)
        {
            throw new InvalidOperationException($"The from value ({from}) can only be equal to the until version ({until}) if the `UntilInclusive` option is set.");
        }
    }

    /// <summary>
    /// Applies the "version >= from" and "version &lt;= / &lt; until" filters based on the options.
    /// A <c>null</c> bound is treated as unbounded on that side.
    /// </summary>
    private IEnumerable<ApiVersion> ApplyVersionFilters(ApiVersion? from, ApiVersion? until, IEnumerable<ApiVersion> apiVersions)
    {
        if (from is ApiVersion fromVersion)
        {
            apiVersions = apiVersions.Where(version => version >= fromVersion);
        }

        if (until is ApiVersion untilVersion)
        {
            apiVersions = options.UntilInclusive
                ? apiVersions.Where(version => version <= untilVersion)
                : apiVersions.Where(version => version < untilVersion);
        }

        return apiVersions;
    }
}
