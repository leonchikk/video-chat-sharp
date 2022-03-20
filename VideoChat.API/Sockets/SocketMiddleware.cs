using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VideoChat.API.Sockets
{
    public class SocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConnectionsManager _connectionManager;
        private readonly SocketHandler _socketHandler;
        //private WebSocketHandler _webSocketHandler { get; set; }

        public SocketMiddleware(RequestDelegate next,
            ConnectionsManager connectionManager,
            SocketHandler socketHandler)
        {
            _next = next;
            _connectionManager = connectionManager;
            _socketHandler = socketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            try
            {
                var accountId = context.User.Claims.FirstOrDefault(x => x.Type == "AccountId")?.Value;
                var socket = await context.WebSockets.AcceptWebSocketAsync();

                if (string.IsNullOrEmpty(accountId))
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Forced termination", CancellationToken.None);

                _connectionManager.AddConnection(accountId, socket);

                await _socketHandler.HandleIncomings(socket);
            }
            catch (Exception ex)
            {
            }

            //TODO - investigate the Kestrel exception thrown when this is the last middleware
            //await _next.Invoke(context);
        }
    }
}
