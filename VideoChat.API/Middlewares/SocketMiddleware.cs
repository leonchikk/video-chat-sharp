using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;
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

            var accountId = context.User.Claims.FirstOrDefault(x => x.Type == "AccountId")?.Value;
            _socket = new WebSocketAdapter(await context.WebSockets.AcceptWebSocketAsync());

            if (string.IsNullOrEmpty(accountId))
            {
                await _socket.Close();
                return;
            }

            _socket.OnMessage += OnMessage;
            _connectionManager.Add(new Connection(accountId, _socket));

            await _broadcaster.ToUser(accountId, new UsersListPacket(
                _connectionManager.Get()
                                  .Where(x => x.AccountId != accountId)
                                  .Select(x => x.AccountId)
                                  .ToArray()
                ));
            await _broadcaster.ToAllExcept(accountId, new UserConnectionPacket(accountId));
            await _socket.HandleIncomings();
            await _broadcaster.ToAllExcept(accountId, new UserDisconnectPacket(accountId));

            _connectionManager.Remove(accountId);
        }

        private void OnMessage(NetworkMessageReceivedEventArgs obj)
        {
            switch (obj.PacketType)
            {
                case PacketTypeEnum.Video:
                    break;
                case PacketTypeEnum.Audio:

                    var audioPacket = PacketConvertor.ToAudioPacket(obj.PacketPayload);
                    _broadcaster.ToAllExcept(audioPacket.SenderId, obj.RawPayload);

                    break;
                case PacketTypeEnum.Event:
                    break;
                case PacketTypeEnum.UserList:
                    break;
                default:
                    break;
            }
        }
    }
}
