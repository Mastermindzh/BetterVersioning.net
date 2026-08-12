using BetterVersioning.Net.Attributes;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("v{version:apiVersion}/replacement")]
[From(1)]
public class VersionReplacementController : ControllerBase
{
    [HttpGet]
    [Until(2)]
    public string Legacy() => "Legacy implementation";

    [HttpGet]
    [From(6)]
    public string Current() => "Current implementation";
}
