using BetterVersioning.Net.Attributes;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("v{version:apiVersion}/method-only")]
public class MethodOnlyController : ControllerBase
{
    [HttpGet("introduced")]
    [From(34)]
    public string Introduced() => "Available from version 34 without a controller-level version attribute.";

    [HttpGet("removed")]
    [Until(2)]
    public string Removed() => "Available through version 2 without a controller-level version attribute.";
}
