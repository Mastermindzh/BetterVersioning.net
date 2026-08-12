# [Betterversioning.net](https://github.com/Mastermindzh/BetterVersioning.net)

[![Build Status](https://ci.mastermindzh.tech/api/badges/Mastermindzh/BetterVersioning.net/status.svg)](https://ci.mastermindzh.tech/Mastermindzh/BetterVersioning.net) ![Nuget](https://img.shields.io/nuget/dt/BetterVersioning.net) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Mastermindzh_BetterVersioning.net&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Mastermindzh_BetterVersioning.net) [![https://img.shields.io/badge/view_on-Github-blue](https://img.shields.io/badge/view_on-Github-blue)](https://github.com/Mastermindzh/BetterVersioning.net)

BetterVersioning.net is an opinionated convention for the versioning of Web APIs. It allows you to add versions in a "from &amp; until" manner instead of an attribute for every version. See the [Using it](#using-it) chapter.

For complete examples see the [examples](https://github.com/Mastermindzh/BetterVersioning.net/tree/main/examples) folder.

<!-- toc -->

- [Betterversioning.net](#betterversioningnet)
  - [Explanation](#explanation)
  - [Why](#why)
  - [Installation](#installation)
  - [Using it](#using-it)
  - [Choosing a document generator + UI](#choosing-a-document-generator--ui)
    - [Swashbuckle + Swagger UI](#swashbuckle--swagger-ui)
    - [Microsoft OpenAPI + Scalar](#microsoft-openapi--scalar)
    - [Microsoft OpenAPI + Swagger UI](#microsoft-openapi--swagger-ui)
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

The core convention package:

`dotnet add package BetterVersioning.net`

Optional companion packages wire up one versioned API document (and UI) per configured version:

| Package                            | Adds                                                 |
| ---------------------------------- | ---------------------------------------------------- |
| `BetterVersioning.net.Swashbuckle` | Swashbuckle document generation + Swagger UI         |
| `BetterVersioning.net.OpenApi`     | Microsoft OpenAPI (`AddOpenApi`) document generation |
| `BetterVersioning.net.Scalar`      | Scalar UI over the Microsoft OpenAPI documents       |

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
    using Asp.Versioning;

    builder.Services.AddApiVersioning(opt =>
    {
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                        new HeaderApiVersionReader("x-api-version"),
                                                        new MediaTypeApiVersionReader("x-api-version"));

        opt.DefaultApiVersion = new ApiVersion(32, 0);
    })
    .AddMvc(options =>
    {
        // Add the convention
        options.Conventions = new BetterVersioningConventionBuilder(versions, new BetterVersioningOptions() { UntilInclusive = true });
    })
    .AddApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
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

## Choosing a document generator + UI

BetterVersioning.net only assigns versions to your controllers/methods; it is UI-agnostic.
The companion packages turn the configured `BetterVersion[]` into one API document per
version (deprecation notices included). Pick **either** document generator and **either** UI:

| Document generator               | UI         | Packages                                                            |
| -------------------------------- | ---------- | ------------------------------------------------------------------- |
| Swashbuckle (`SwaggerGen`)       | Swagger UI | `BetterVersioning.net.Swashbuckle`                                  |
| Microsoft OpenAPI (`AddOpenApi`) | Scalar     | `BetterVersioning.net.OpenApi` + `BetterVersioning.net.Scalar`      |
| Microsoft OpenAPI (`AddOpenApi`) | Swagger UI | `BetterVersioning.net.OpenApi` + `Swashbuckle.AspNetCore.SwaggerUI` |

Both derive document/group names from the same `BetterVersion[]` and
`ApiExplorerOptions.GroupNameFormat`, so naming stays consistent with routing.

### Swashbuckle + Swagger UI

```csharp
using BetterVersioning.Net.Swashbuckle;

builder.Services.AddBetterVersioningSwagger(options => options.Title = "My API");

var app = builder.Build();

app.UseBetterVersioningSwaggerUI(); // one Swagger endpoint per version
```

### Microsoft OpenAPI + Scalar

```csharp
using BetterVersioning.Net.OpenApi;
using BetterVersioning.Net.Scalar;

builder.Services.AddBetterVersioningOpenApi(versions, options => options.Title = "My API");

var app = builder.Build();

app.MapBetterVersioningOpenApi();       // /openapi/{documentName}.json per version
app.MapBetterVersioningScalar(); // Scalar UI at /scalar with a version selector
```

### Microsoft OpenAPI + Swagger UI

The generated documents are UI-agnostic, so you can keep Swagger UI while using the
Microsoft generator. Reference `Swashbuckle.AspNetCore.SwaggerUI` (UI only, no generator)
and point it at the per-version documents:

```csharp
using BetterVersioning.Net.OpenApi;

builder.Services.AddBetterVersioningOpenApi(versions, options => options.Title = "My API");

var app = builder.Build();

app.MapBetterVersioningOpenApi(); // /openapi/{documentName}.json per version
app.UseSwaggerUI(ui =>
{
    foreach (var document in app.Services.GetVersionedDocuments())
    {
        ui.SwaggerEndpoint($"/openapi/{document.GroupName}.json", document.GroupName);
    }
});
```

> The Microsoft OpenAPI and Scalar packages target `net10.0`. The core and Swashbuckle
> packages target `net9.0;net10.0`.

## Defining versions

The main parameter for the convention is the "versions" array.
This array can be declared wherever you want and consists of BetterVersion objects.

Each object is constructed with 3 main parameters:

| Name          | Type            | Description                                                          |
| ------------- | --------------- | -------------------------------------------------------------------- |
| MajorVersion  | ushort          | The major version for this version                                   |
| MinorVersions | array of ushort | The minor versions you want included in this version                 |
| Supported     | boolean         | Whether the version is still supported (false would mean deprecated) |

All unsupported (/deprecated) versions are still usable but will get the deprecation message (if set up in OpenApi) applied.  

## Convention options

When you add the convention you can (optionally) pass an options object that contains configuration options for the convention. All options are listed in the table below.

| Name                      | Type    | Description                                                                            |
| ------------------------- | ------- | -------------------------------------------------------------------------------------- |
| UntilInclusive            | boolean | Whether the `[Until]` attribute is inclusive or exclusive of the given version number. |
| DetectDuplicatesAtStartup | boolean | Whether BetterVersioning.net checks for, and errors out if, duplicates when it starts  |
