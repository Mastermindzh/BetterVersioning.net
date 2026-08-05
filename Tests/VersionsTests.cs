using Microsoft.AspNetCore.Mvc;

using BetterVersioning;
using BetterVersioning.Net.Models;

using FluentAssertions;

namespace BetterVersioning.net.Tests;

public class VersionsTests
{
  private static readonly BetterVersion[] SampleVersions =
  {
        new(1, supported: false),            // deprecated: 1.0
        new(2, new ushort[] { 1 }),          // supported:  2.0, 2.1
        new(3),                              // supported:  3.0
        new(4, new ushort[] { 1 }, false),   // deprecated: 4.0, 4.1
    };

  private static Versions Build(bool untilInclusive = false) =>
      new(SampleVersions, new BetterVersioningOptions { UntilInclusive = untilInclusive });

  [Fact]
  public void GetVersions_WithoutFrom_TreatsLowerBoundAsUnbounded()
  {
    // [Until]-only (from == null) used to throw; it should now filter on the upper bound only.
    var (supported, deprecated) = Build(untilInclusive: true).GetVersions(from: null, until: new ApiVersion(2, 0));

    supported.Should().BeEquivalentTo(new[] { new ApiVersion(2, 0) });
    deprecated.Should().BeEquivalentTo(new[] { new ApiVersion(1, 0) });
  }

  [Fact]
  public void GetVersions_WithoutFrom_ExclusiveUntil_ExcludesUpperBound()
  {
    var (supported, deprecated) = Build(untilInclusive: false).GetVersions(from: null, until: new ApiVersion(2, 0));

    supported.Should().BeEmpty();
    deprecated.Should().BeEquivalentTo(new[] { new ApiVersion(1, 0) });
  }

  [Fact]
  public void GetVersions_WithoutUntil_TreatsUpperBoundAsUnbounded()
  {
    var (supported, deprecated) = Build().GetVersions(from: new ApiVersion(2, 0), until: null);

    supported.Should().BeEquivalentTo(new[] { new ApiVersion(2, 0), new ApiVersion(2, 1), new ApiVersion(3, 0) });
    deprecated.Should().BeEquivalentTo(new[] { new ApiVersion(4, 0), new ApiVersion(4, 1) });
  }

  [Fact]
  public void GetVersions_FromEqualsUntil_WithUntilInclusive_ReturnsThatVersion()
  {
    var (supported, _) = Build(untilInclusive: true)
        .GetVersions(from: new ApiVersion(2, 0), until: new ApiVersion(2, 0));

    supported.Should().BeEquivalentTo(new[] { new ApiVersion(2, 0) });
  }

  [Fact]
  public void GetVersions_FromEqualsUntil_WithoutUntilInclusive_Throws()
  {
    var act = () => Build(untilInclusive: false)
        .GetVersions(from: new ApiVersion(2, 0), until: new ApiVersion(2, 0));

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void GetVersions_FromGreaterThanUntil_Throws()
  {
    var act = () => Build(untilInclusive: true)
        .GetVersions(from: new ApiVersion(3, 0), until: new ApiVersion(2, 0));

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Constructor_WithDuplicateMajorVersions_Throws()
  {
    var act = () => new Versions(
        new[] { new BetterVersion(1), new BetterVersion(1) },
        new BetterVersioningOptions());

    act.Should().Throw<ArgumentException>();
  }
}
