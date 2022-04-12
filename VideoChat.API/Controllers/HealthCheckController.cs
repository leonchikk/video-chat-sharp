﻿using Microsoft.AspNetCore.Mvc;
using VideoChat.API.Sockets;

namespace VideoChat.API.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ConnectionsManager _connectionManager;

        public HealthCheckController(ConnectionsManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok($"API is available. Current online : {_connectionManager.UsersConnections.Count}");
        }
    }
}