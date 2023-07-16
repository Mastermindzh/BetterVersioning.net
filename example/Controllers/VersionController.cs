using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
public class VersionController : ControllerBase
{
    [MapToApiVersion("1.0")]
    [HttpGet]
    public string Get1() => "1";

    [MapToApiVersion("2.0")]
    [HttpGet]
    public string Get2() => "2";

}
