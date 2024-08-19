# [Betterversioning.net](https://github.com/Mastermindzh/BetterVersioning.net)

[![Build Status](https://ci.mastermindzh.tech/api/badges/Mastermindzh/BetterVersioning.net/status.svg)](https://ci.mastermindzh.tech/Mastermindzh/BetterVersioning.net) ![Nuget](https://img.shields.io/nuget/dt/BetterVersioning.net) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![https://img.shields.io/badge/view_on-Github-blue](https://img.shields.io/badge/view_on-Github-blue)](https://github.com/Mastermindzh/BetterVersioning.net)

BetterVersioning.net is an opinionated convention for the versioning of Web APIs. It allows you to add versions in a "from &amp; until" manner instead of an attribute for every version. See the [Using it](#using-it) chapter.

For a complete example see the [example](https://github.com/Mastermindzh/BetterVersioning.net/tree/main/example) folder.

<!-- toc -->

- [Betterversioning.net](#betterversioningnet)
  - [Explanation](#explanation)
  - [Why](#why)
  - [Installation](#installation)
  - [Using it](#using-it)
  - [Defining versions](#defining-versions)
  - [Convention options](#convention-options)

<!-- tocstop -->

## Explanation

BetterVersioning.net works by adding API versions in a `from & until` manner instead of manually specifying every version on a controller/method.

Specifying `[From(1,0)]` would mean this controller/method is available in **every** version that you have added to BetterVersioning.net that is version 1.0 or higher.

Adding an `[Until(5,0)]` to the same controller/method will ensure that the controller/method will no longer show up on (or after) version 5.0. (depending on options)

## Why

Managing multiple versions of an API with the regular `[ApiVersion(1.0)]` and `[MapToApiVersion("1.0")]` attributes can become cumbersome as the number of versions inside of your API increase.

The main reasons I initially came up with this versioning idea are:

- No need to modify controllers that are not changed when you add a new version
- Versions are automatically added/removed from controllers based on a convention
- Simplify multiple supported/deprecated versions

## Installation

`dotnet add package BetterVersioning.net`

## Using it

1. Define your API versions in the following format:

    ```csharp
      var versions = new[] {
        new BetterVersion(32, new ushort[]{1,2}, supported: false),
        new BetterVersion(33, supported: false)
      };
    ```

2. Modify the `builder.Services.AddApiVersioning` block by adding the BetterVersioning convention:

    ```csharp
    builder.Services.AddApiVersioning(opt =>
    {
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                        new HeaderApiVersionReader("x-api-version"),
                                                        new MediaTypeApiVersionReader("x-api-version"));

        opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(32, 0);

        // set up versions
        var versions = new[] {
            new BetterVersion(31, new ushort[]{1,2,3}),
            new BetterVersion(32),
        };

        // Add the convention
        opt.Conventions = new BetterVersioningConventionBuilder(versions, new BetterVersioningOptions() { UntilInclusive = true });
    });
    ```

3. Add a `[From]` and/or `[Until]` attribute to your controller or method. The code below shows several examples.

    ```csharp
      [ApiController]
      [Route("v{version:apiVersion}/[controller]")]
      [From(6)]
      public class BetterVersionsController : ControllerBase
      {

          [HttpGet]
          public string Get1() => "This endpoint was introduced in version 6";

          [HttpGet("minor-range")]
          [Until(6, 1)]
          public string Get2() => "This endpoint was introduced in version 6.0 and removed after version 6.1";

          [HttpGet("new")]
          [From(31)]
          [Until(33)]
          public string Get3() => "This endpoint was introduced in version 31 and removed after version 33";

          [HttpGet("new-only-minors")]
          [From(34, 1)]
          [Until(34, 2)]
          public string Get4() => "This endpoint was introduced in version 34.1 and removed after version 34.2";

          [HttpGet("new-till-31")]
          [Until(31)]
          public string Get5() => "This endpoint was introduced in version 6 and removed after version 31.0";
      }

    ```

## Defining versions

The main parameter for the convention is the "versions" array.
This array can be declared wherever you want and consists of BetterVersion objects.

Each object is constructed with 3 main parameters:

| Name           | Type    | Description                                                                            |
| -------------- | ------- | -------------------------------------------------------------------------------------- |
| MajorVersion | ushort | The major version for this version |
| MinorVersions | array of ushort | The minor versions you want included in this version |
| Supported | boolean | Whether the version is still supported (false would mean deprecated) |

All unsupported (/deprecated) versions are still usable but will get the deprecation message (if set up in OpenApi) applied.  

## Convention options

When you add the convention you can (optionally) pass an options object that contains configuration options for the convention. All options are listed in the table below.

| Name           | Type    | Description                                                                            |
| -------------- | ------- | -------------------------------------------------------------------------------------- |
| UntilInclusive | boolean | Whether the `[Until]` attribute is inclusive or exclusive of the given version number. |
