using Asp.Versioning;

using BetterVersioning;
using BetterVersioning.Net.Models;
using BetterVersioning.Net.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

// set up versions
var versions = new[] {
  new BetterVersion(1, supported: false),
  new BetterVersion(2, supported: false),
  new BetterVersion(6, [1],supported: false),
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

builder.Services.AddBetterVersioningSwagger(options => options.Title = "BetterVersioning.net example (Swagger UI)");

var app = builder.Build();

app.UseBetterVersioningSwaggerUI();

app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();
