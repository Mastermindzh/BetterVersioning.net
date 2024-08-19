using Microsoft.AspNetCore.Mvc;

using BetterVersioning.Net.Attributes;

namespace WebApi.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
[From(6)]
public class BetterVersionsController : ControllerBase
{
    [HttpGet]
    public string Get1() => "This endpoint was introduced in version 6";

    [HttpGet("until-inclusive-test"), Until(6)]
    public string GetTest() => "This endpoint will only work with UntilInclusive = true";

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
