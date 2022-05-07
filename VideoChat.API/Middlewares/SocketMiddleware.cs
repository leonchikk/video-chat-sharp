using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets.Events;
using VoiceEngine.Network.Abstractions.Server;
using VoiceEngine.Network.Abstractions.Server.Models;
using VoiceEngine.Network.Server.SocketAdapters;

namespace VoiceEngine.API.Middlewares
{
    public class SocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionManager _connectionManager;
        private readonly IBroadcaster _broadcaster;
        private ISocket _socket;
        private string _accountId;

        public SocketMiddleware(
            RequestDelegate next,
            IConnectionManager connectionManager,
            IBroadcaster broadcaster)
        {
            _next = next;
            _connectionManager = connectionManager;
            _broadcaster = broadcaster;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            _accountId = context.User.Claims.FirstOrDefault(x => x.Type == "AccountId")?.Value;
            _socket = new WebSocketAdapter(await context.WebSockets.AcceptWebSocketAsync());

            if (string.IsNullOrEmpty(_accountId))
            {
                await _socket.Close();
                return;
            }

            _socket.OnMessage += OnMessage;
            _connectionManager.Add(new Connection(_accountId, _socket));

            await _broadcaster.ToAllExcept(_accountId, new UserConnectionPacket(_accountId));
            await _socket.HandleIncomings();
            await _broadcaster.ToAllExcept(_accountId, new UserDisconnectPacket(_accountId));

            _connectionManager.Remove(_accountId);
        }

        private void OnMessage(NetworkMessageReceivedEventArgs obj)
        {
            _broadcaster.ToAllExcept(_accountId, obj.RawPayload);
        }
    }
}
