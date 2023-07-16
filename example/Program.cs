using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

using BetterVersioning;
using BetterVersioning.Net.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                    new HeaderApiVersionReader("x-api-version"),
                                                    new MediaTypeApiVersionReader("x-api-version"));

    opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(37, 0);

    // set up versions
    var versions = new[] {
      new BetterVersion(1, supported: false),
      new BetterVersion(2, supported: false),
      new BetterVersion(6, supported: false),
      new BetterVersion(31, new ushort[]{1,2,3}, supported: false),
      new BetterVersion(32, new ushort[]{1}, supported: false),
      new BetterVersion(33, supported: false),
      new BetterVersion(34, new ushort[]{1,2,3,4,5,6}),
      new BetterVersion(37),
    };

    // Add the convention
    opt.Conventions = new BetterVersioningConventionBuilder(versions, new BetterVersioningOptions() { UntilInclusive = true });
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });


app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();
