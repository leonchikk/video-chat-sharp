using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Server;

namespace VoiceEngine.Network.Server.SocketAdapters
{
    public class WebSocketAdapter : ISocket
    {
        private bool _disposed = false;
        private byte[] _buffer = new byte[4096];
        private readonly WebSocket _webSocket;

        public event Action<NetworkMessageReceivedEventArgs> OnMessage;
        public event Action OnDisconnect;

        public WebSocketAdapter(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public Task Close()
        {
            return _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _webSocket.Dispose();
            _disposed = true;
        }

        public Task Send(Packet packet)
        {
            return _webSocket.SendAsync(packet.Payload(), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public Task Send(byte[] packet)
        {
            return _webSocket.SendAsync(packet, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public async Task HandleIncomings()
        {
            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {

                    var result = await _webSocket.ReceiveAsync(buffer: new ArraySegment<byte>(_buffer), cancellationToken: CancellationToken.None);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[result.Count];
                        Buffer.BlockCopy(_buffer, 0, buffer, 0, result.Count);

                        var packetTypeEnum = (PacketTypeEnum)buffer[0];
                        var payload = new byte[buffer.Length - 1]; //Get rid of packet type byte
                        Buffer.BlockCopy(buffer, 1, payload, 0, payload.Length);

                        OnMessage?.Invoke(new NetworkMessageReceivedEventArgs(packetTypeEnum, payload, buffer));
                    }
                }
            }
            finally
            {
                OnDisconnect?.Invoke();
            }
        }
    }
}
