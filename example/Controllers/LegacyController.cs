using Microsoft.AspNetCore.Mvc;

using BetterVersioning.Net.Attributes;

namespace WebApi.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[Until(31)]
public class LegacyController : ControllerBase
{
    [HttpGet]
    public string Get() => "Available in every version up to and including 31, without needing a [From].";
}
