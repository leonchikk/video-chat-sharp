using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Networking;
using VideoChat.Core.Packets;

namespace Networking
{
    public class WebSocketClient : IWebSocketClient
    {
        //TODO: Move this to the settings
        private readonly string _url = "ws://video-chat-sharp.azurewebsites.net";

        private readonly ClientWebSocket _clientSocket;
        private readonly ConcurrentQueue<Packet> _packetsToSendQueue;

        public event Action<NetworkMessageReceivedEventArgs> OnMessage;
        public event Action OnConnection;
        public event Action OnDisconnect;

        public WebSocketClient()
        {
            _clientSocket = new ClientWebSocket();
            _packetsToSendQueue = new ConcurrentQueue<Packet>();
        }

        public async Task Connect(string jwtToken)
        {
            _clientSocket.Options.SetRequestHeader("Authorization", jwtToken);

            await _clientSocket.ConnectAsync(new Uri(_url), CancellationToken.None).ConfigureAwait(false);

            _ = Task.Run(ReceivePacketsJob).ConfigureAwait(false);
            _ = Task.Run(ProccesPacketsToSendQueueJob).ConfigureAwait(false);

            OnConnection?.Invoke();
        }

        public async Task Disconnect()
        {
            await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            OnDisconnect?.Invoke();
        }

        public void SendPacket(Packet packet)
        {
            _packetsToSendQueue.Enqueue(packet);
        }

        private async Task ReceivePacketsJob()
        {
            //List<byte> receivedBytes = new List<byte>();

            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] receivedStorage = new byte[4096];
                    var result = await _clientSocket.ReceiveAsync(buffer: new ArraySegment<byte>(receivedStorage), cancellationToken: CancellationToken.None);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[result.Count];
                        Buffer.BlockCopy(receivedStorage, 0, buffer, 0, result.Count);

                        var packetTypeEnum = (PacketTypeEnum) buffer[0];
                        var payload = new byte[buffer.Length - 1]; //Get rid of packet type byte
                        Buffer.BlockCopy(buffer, 1, payload, 0, payload.Length);

                        OnMessage?.Invoke(new NetworkMessageReceivedEventArgs(packetTypeEnum, payload));
                    }
                }
                //TODO: Add logger
                catch (Exception ex)
                {
                    continue;
                }
            }

            await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        private async Task ProccesPacketsToSendQueueJob()
        {
            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    _packetsToSendQueue.TryDequeue(out var packet);

                    if (packet == null)
                        continue;

                    //Note: websocket client cannot send simultaneously packets
                    await _clientSocket.SendAsync(
                            new ArraySegment<byte>(packet.Payload()),
                            WebSocketMessageType.Binary,
                            true,
                            CancellationToken.None);
                }
                //TODO: Add logger
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public void Dispose()
        {
            _clientSocket.Dispose();
        }
    }
}
