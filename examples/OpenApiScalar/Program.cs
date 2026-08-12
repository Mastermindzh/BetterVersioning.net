using Asp.Versioning;

using BetterVersioning;
using BetterVersioning.Net.Models;
using BetterVersioning.Net.OpenApi;
using BetterVersioning.Net.Scalar;

var builder = WebApplication.CreateBuilder(args);

// set up versions (the single source of truth, shared by the convention and the OpenAPI documents)
var versions = new[] {
  new BetterVersion(1, supported: false),
  new BetterVersion(2, supported: false),
  new BetterVersion(6, [1], supported: false),
  new BetterVersion(31, [1,2,3], supported: false),
  new BetterVersion(32, [1], supported: false),
  new BetterVersion(33, supported: false),
  new BetterVersion(34, [1,2,3,4,5,6]),
  new BetterVersion(37),
};

builder.Services.AddControllers();
builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                    new HeaderApiVersionReader("x-api-version"),
                                                    new MediaTypeApiVersionReader("x-api-version"));

    opt.DefaultApiVersion = new ApiVersion(37, 0);
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

// One Microsoft OpenAPI document per version, deprecation metadata included.
builder.Services.AddBetterVersioningOpenApi(versions, options => options.Title = "BetterVersioning.net example (Scalar)");

var app = builder.Build();

// Serve /openapi/{documentName}.json for every version and the Scalar UI at /scalar.
app.MapBetterVersioningOpenApi();
app.MapBetterVersioningScalar();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so the integration tests can host this app with WebApplicationFactory.
public partial class Program;
