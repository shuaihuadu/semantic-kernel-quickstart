using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Web.Controllers;

[ApiController]
[Route("api/plugins/[controller]/[action]")]
public class AppController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public string Ping()
    {
        return "Pong";
    }
}