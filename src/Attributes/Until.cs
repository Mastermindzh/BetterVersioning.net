using Microsoft.AspNetCore.Mvc;

namespace BetterVersioning.Net.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class Until : Attribute
{
    public ApiVersion Version { get; }

    /// <summary>
    /// Apply this attribute to a controller or method to denote in what version it was removed
    /// </summary>
    /// <param name="majorVersion"></param>
    /// <param name="minorVersion"></param>
    public Until(ushort majorVersion, ushort minorVersion = 0)
    {
        Version = new ApiVersion(majorVersion, minorVersion);
    }
}
