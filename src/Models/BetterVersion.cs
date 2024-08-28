namespace BetterVersioning.Net.Models;

/// <summary>
/// A representation of a major version containing one or more minor versions, that can be supported.
/// </summary>
public record BetterVersion
{
    /// <summary>
    /// The major version this code should appear in.
    /// </summary>
    /// <value></value>
    public ushort MajorVersion { get; }

    /// <summary>
    /// A list of minor versions you want included in this version.
    /// </summary>
    /// <remarks>Minor version 0 is implied and does not need to be added to the array of minor versions.</remarks>
    public ushort[] MinorVersions { get; }

    /// <summary>
    /// Whether the version is still supported.
    /// </summary>
    /// <remarks><c>false</c> would mean deprecated, default is <c>true</c>.</remarks>
    public bool Supported { get; }

    /// <summary>
    /// Creates a new <see cref="BetterVersion"/> with a minor version of 0 added implicitly.
    /// </summary>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersions">Any additional minor versions.</param>
    /// <param name="supported">Whether this major version is supported.</param>
    /// <exception cref="ArgumentException"><paramref name="minorVersions"/> contains duplicates.</exception>
    public BetterVersion(ushort majorVersion, ushort[]? minorVersions = null, bool? supported = true)
    {
        if (minorVersions is not null && minorVersions.Length != minorVersions.Distinct().Count())
        {
            throw new ArgumentException("Minor versions contains duplicates", nameof(minorVersions));
        }

        MajorVersion = majorVersion;

        // Add version `x.0` as minor version because it is implied.
        MinorVersions = minorVersions is null
            ? new ushort[] { 0 }
            : new ushort[] { 0 }.Intersect(minorVersions).ToArray();
        Supported = supported ?? true;
    }

}
