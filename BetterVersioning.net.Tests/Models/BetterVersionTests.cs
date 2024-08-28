using BetterVersioning.Net.Models;
using FluentAssertions;

namespace BetterVersioning.net.Tests.Models;

public class BetterVersionTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void BetterVersions_Has_MinorVersion_Zero_When_Constructed_With_No_MinorVersions(
        ushort majorVersion,
        bool? supported)
    {
        // Act
        var betterVersion = new BetterVersion(majorVersion, supported: supported);
        
        // Assert
        betterVersion.MinorVersions.Should()
            .ContainSingle("No other versions were given")
            .And.Contain(0, "Zero is implicitly added");
    }
    
    [Theory]
    [InlineData(1, true, new ushort[] {1})]
    [InlineData(2, false, new ushort[] {1, 2})]
    public void BetterVersions_Has_MinorVersion_Zero_And_Given_When_Constructed_With_MinorVersions_Without_Zero(
        ushort majorVersion,
        bool? supported,
        ushort[] minorVersions)
    {
        // Act
        var betterVersion = new BetterVersion(majorVersion, minorVersions, supported: supported);
        
        // Assert
        betterVersion.MinorVersions.Should()
            .HaveCount(minorVersions.Length + 1, "Zero was added to the given list")
            .And.Contain(0, "Zero is implicitly added as a version")
            .And.Contain(minorVersions, "All given minor versions should be added");
    }
    
    [Theory]
    [InlineData(1, true, new ushort[] {0})]
    [InlineData(2, false, new ushort[] {0, 1})]
    public void BetterVersions_Has_MinorVersion_Given_Versions_When_Constructed_With_MinorVersions_With_Zero(
        ushort majorVersion,
        bool? supported,
        ushort[] minorVersions)
    {
        // Act
        var betterVersion = new BetterVersion(majorVersion, minorVersions, supported: supported);
        
        // Assert
        betterVersion.MinorVersions.Should()
            .HaveCount(minorVersions.Length, "Zero was already in the given list")
            .And.Contain(0, "Zero should always be a minor version")
            .And.Contain(minorVersions, "All given minor versions should be added");
    }
}
