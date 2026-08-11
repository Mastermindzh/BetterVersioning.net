using Asp.Versioning;

namespace BetterVersioning.Net.Attributes;

/// <summary>
/// Apply this attribute to a controller or method to denote in what version it was removed
/// </summary>
/// <param name="majorVersion"></param>
/// <param name="minorVersion"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class Until(ushort majorVersion, ushort minorVersion = 0) : Attribute
{
    public ApiVersion Version { get; } = new ApiVersion(majorVersion, minorVersion);
}
