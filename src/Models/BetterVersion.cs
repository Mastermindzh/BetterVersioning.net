namespace BetterVersioning.Net.Models;

public class BetterVersion
{
    /// <summary>
    /// The major version this code should appear in
    /// </summary>
    /// <value></value>
    public ushort MajorVersion { get; set; }

    /// <summary>
    /// A list of minor versions you want included in this version
    /// </summary>
    /// <value></value>
    public ushort[]? MinorVersions { get; set; }

    /// <summary>
    /// Whether the version is still supported (false would mean deprecated)
    /// </summary>
    /// <value></value>
    public bool? Supported { get; }

    public BetterVersion(ushort majorVersion, ushort[]? minorVersions = null, bool? supported = true)
    {
        MajorVersion = majorVersion;
        MinorVersions = minorVersions;
        Supported = supported;
    }

}
