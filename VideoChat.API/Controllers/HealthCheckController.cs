using Microsoft.AspNetCore.Mvc;
using VideoChat.API.Sockets;

namespace VideoChat.API.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ConnectionManager _connectionManager;

        public HealthCheckController(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok($"API is available. Current online : {_connectionManager.Connections.Count}");
        }
    }
}
