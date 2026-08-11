using Asp.Versioning;
using BetterVersioning;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BetterVersioning.net.Tests;

public class DuplicateDetectionTests
{
    [Fact]
    public void HasDuplicateEndpoints_SameRouteDifferentVerbs_ReturnsFalse()
    {
        var endpoints = new[]
        {
            Endpoint("GET", "resource", 1),
            Endpoint("POST", "resource", 1),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeFalse();
    }

    [Fact]
    public void HasDuplicateEndpoints_SameRouteSameVerb_ReturnsTrue()
    {
        var endpoints = new[]
        {
            Endpoint("GET", "resource", 1),
            Endpoint("GET", "resource", 1),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeTrue();
    }

    [Fact]
    public void HasDuplicateEndpoints_RouteDifferencesOnlyInCase_ReturnsTrue()
    {
        var endpoints = new[]
        {
            Endpoint("GET", "Resource", 1),
            Endpoint("get", "resource", 1),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeTrue();
    }

    [Fact]
    public void HasDuplicateEndpoints_DistinctRoutes_ReturnsFalse()
    {
        var endpoints = new[]
        {
            Endpoint("GET", "a", 1),
            Endpoint("GET", "b", 1),
            Endpoint("POST", "a", 1),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeFalse();
    }

    [Fact]
    public void HasDuplicateEndpoints_SameRouteAndVerbInDisjointVersions_ReturnsFalse()
    {
        var endpoints = new[]
        {
            Endpoint("GET", "resource", 1, 2),
            Endpoint("GET", "resource", 6, 31),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeFalse();
    }

    [Fact]
    public void HasDuplicateEndpoints_PartiallyOverlappingVerbSets_ReturnsTrue()
    {
        var versions = new HashSet<ApiVersion> { new(1, 0) };
        var endpoints = BetterVersioningConventionBuilder.GetVersionedEndpoints(
                [
                    new AcceptVerbsAttribute(["GET", "POST"]) { Route = "resource" },
                    new HttpGetAttribute("resource"),
                ],
                hasControllerRoute: true,
                versions)
            .ToArray();

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(endpoints).Should().BeTrue();
    }

    [Fact]
    public void HasDuplicateEndpoints_SeparateRouteAttributesRemainDistinct()
    {
        var versions = new HashSet<ApiVersion> { new(1, 0) };
        var first = BetterVersioningConventionBuilder.GetVersionedEndpoints(
            [new HttpGetAttribute(), new RouteAttribute("a")],
            hasControllerRoute: true,
            versions);
        var second = BetterVersioningConventionBuilder.GetVersionedEndpoints(
            [new HttpGetAttribute(), new RouteAttribute("b")],
            hasControllerRoute: true,
            versions);

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(first.Concat(second)).Should().BeFalse();
    }

    private static BetterVersioningConventionBuilder.VersionedEndpoint Endpoint(
        string method,
        string route,
        params int[] majorVersions) =>
        new(
            method,
            route,
            majorVersions.Select(major => new ApiVersion(major, 0)).ToHashSet());
}
