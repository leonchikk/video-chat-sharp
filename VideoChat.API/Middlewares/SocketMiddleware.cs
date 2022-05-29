using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;
using VoiceEngine.Network.Abstractions.Packets.Events;
using VoiceEngine.Network.Abstractions.Server;
using VoiceEngine.Network.Abstractions.Server.Extensions;
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

                case PacketTypeEnum.Event:

                    var eventPacket = PacketConvertor.ToEventPacket(obj.PacketPayload);

                    string senderId = null;
                    Packet packet = null;

                    switch (eventPacket.EventType)
                    {
                        case EventTypeEnum.Mute:
                            packet = PacketConvertor.ToMutePacket(eventPacket.PacketPayload);
                            senderId = (packet as MutePacket).AccountId;
                            break;

                        case EventTypeEnum.Unmute:
                            packet = PacketConvertor.ToUnmutePacket(eventPacket.PacketPayload);
                            senderId = (packet as UnmutePacket).AccountId;
                            break;
                    }

                    _ = _broadcaster.ToAllExcept(senderId, packet);

                    break;

                case PacketTypeEnum.FinishHandshake:

                    var finishHandshakePacket = PacketConvertor.ToFinishHandshakePacket(obj.PacketPayload);
                    var sender = _connectionManager.Get(finishHandshakePacket.SenderId);

                    sender.NickName = finishHandshakePacket.Nickname;

                    _ = _broadcaster.ToUser(finishHandshakePacket.SenderId, new UsersListPacket
                                        (
                                            _connectionManager.GetAllExept(finishHandshakePacket.SenderId)
                                        ));

                    _ = _broadcaster.ToAllExcept(finishHandshakePacket.SenderId, new UserConnectionPacket(finishHandshakePacket.SenderId, finishHandshakePacket.Nickname));

                    break;

                default:
                    break;
            }
        }
    }
}
