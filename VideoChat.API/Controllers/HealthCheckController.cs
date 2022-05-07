using Microsoft.AspNetCore.Mvc;
using VoiceEngine.Network.Abstractions.Server;

namespace VoiceEngine.API.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly IConnectionManager _connectionManager;

        public HealthCheckController(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok($"API is available. Current online: {_connectionManager.Online}");
        }
    }
}
