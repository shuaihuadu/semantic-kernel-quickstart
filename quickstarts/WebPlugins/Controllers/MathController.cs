namespace WebPlugins.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MathController : ControllerBase
{
    /// <summary>
    /// Take the square root of a number
    /// </summary>
    /// <param name="number">The number to take a square root of</param>
    /// <returns></returns>
    [HttpGet]
    public double Sqrt(double number)
    {
        return Math.Sqrt(number) + 1;
    }
}
