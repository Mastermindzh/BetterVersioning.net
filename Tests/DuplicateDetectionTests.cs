using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using BetterVersioning;

using FluentAssertions;

namespace BetterVersioning.net.Tests;

public class DuplicateDetectionTests
{
    [Fact]
    public void HasDuplicateEndpoints_SameRouteDifferentVerbs_ReturnsFalse()
    {
        var attributes = new HttpMethodAttribute[]
        {
            new HttpGetAttribute("resource"),
            new HttpPostAttribute("resource"),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(attributes).Should().BeFalse();
    }

    [Fact]
    public void HasDuplicateEndpoints_SameRouteSameVerb_ReturnsTrue()
    {
        var attributes = new HttpMethodAttribute[]
        {
            new HttpGetAttribute("resource"),
            new HttpGetAttribute("resource"),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(attributes).Should().BeTrue();
    }

    [Fact]
    public void HasDuplicateEndpoints_NullAndEmptyTemplateSameVerb_ReturnsTrue()
    {
        var attributes = new HttpMethodAttribute[]
        {
            new HttpGetAttribute(),
            new HttpGetAttribute(string.Empty),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(attributes).Should().BeTrue();
    }

    [Fact]
    public void HasDuplicateEndpoints_DistinctRoutes_ReturnsFalse()
    {
        var attributes = new HttpMethodAttribute[]
        {
            new HttpGetAttribute("a"),
            new HttpGetAttribute("b"),
            new HttpPostAttribute("a"),
        };

        BetterVersioningConventionBuilder.HasDuplicateEndpoints(attributes).Should().BeFalse();
    }
}
