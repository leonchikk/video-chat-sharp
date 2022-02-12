using Microsoft.AspNetCore.Mvc;

namespace VideoChat.API.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        public IActionResult HealthCheck()
        {
            return Ok("API is available");
        }
    }
}
