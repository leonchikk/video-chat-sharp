using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
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
            var socket = new WebSocketAdapter(await context.WebSockets.AcceptWebSocketAsync());

            socket.OnMessage += OnMessage;

            if (string.IsNullOrEmpty(accountId))
            {
                await socket.Close();
                return;
            }

            socket.OnDisconnect += async () =>
            {
                if (_connectionManager.Get(accountId) == null)
                    return;

                _connectionManager.Remove(accountId);

                await _broadcaster.ToAllExcept(accountId, new UserDisconnectPacket(accountId));
            };

            _connectionManager.Add(new Connection(accountId, socket));

            await socket.Send(new InitHandshakePacket());
            await socket.HandleIncomings();
        }

        private void OnMessage(NetworkMessageReceivedEventArgs obj)
        {
            switch (obj.PacketType)
            {
                case PacketTypeEnum.Video:
                    break;
                case PacketTypeEnum.Audio:

                    var audioPacket = PacketConvertor.ToAudioPacket(obj.PacketPayload);
                    _ = _broadcaster.ToAllExcept(audioPacket.SenderId, obj.RawPayload);

                    break;
                case PacketTypeEnum.FinishHandshake:

                    var finishHandshakePacket = PacketConvertor.ToFinishHandshakePacket(obj.PacketPayload);

                    _ = _broadcaster.ToUser(finishHandshakePacket.SenderId, new UsersListPacket
                                        (
                                            _connectionManager.Get()
                                                              .Where(x => x.AccountId != finishHandshakePacket.SenderId)
                                                              .Select(x => x.AccountId)
                                                              .ToArray()
                                        ));

                    _ = _broadcaster.ToAllExcept(finishHandshakePacket.SenderId, new UserConnectionPacket(finishHandshakePacket.SenderId));

                    break;

                default:
                    break;
            }
        }
    }
}
