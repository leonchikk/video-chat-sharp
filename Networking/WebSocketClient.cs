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
        private readonly int _tickrate = 1000;
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
            List<byte> receivedBytes = new List<byte>();

            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] tempStorage = new byte[2]; 
                    var result = await _clientSocket.ReceiveAsync(buffer: new ArraySegment<byte>(tempStorage), cancellationToken: CancellationToken.None);

                    receivedBytes.AddRange(tempStorage);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[receivedBytes.Count];
                        for (int i = 0; i < buffer.Length; i++)
                            buffer[i] = receivedBytes[i];

                        receivedBytes.Clear();

                        using (var stream = new MemoryStream(buffer))
                        using (var binaryReader = new BinaryReader(stream))
                        {
                            var packetTypeEnum = (PacketTypeEnum)binaryReader.ReadByte();
                            var payloadLength = binaryReader.ReadInt32();
                            var payload = binaryReader.ReadBytes(payloadLength);

                            OnMessage?.Invoke(new NetworkMessageReceivedEventArgs(packetTypeEnum, payload));
                        }
                    }

                    //await Task.Delay(1000 / _tickrate);
                }
                //TODO: Add logger
                catch (Exception)
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

                    using (var stream = new MemoryStream())
                    {
                        using (var binaryWriter = new BinaryWriter(stream))
                        {
                            var buffer = packet.PayloadBuffer;

                            binaryWriter.Write((byte)packet.Type);
                            binaryWriter.Write(buffer.Length);
                            binaryWriter.Write(buffer);

                            //Note: websocket client cannot send simultaneously packets
                            await _clientSocket.SendAsync(
                                    new ArraySegment<byte>(stream.ToArray()),
                                    WebSocketMessageType.Binary,
                                    true,
                                    CancellationToken.None);

                        }
                    }

                    //await Task.Delay(1000 / _tickrate);
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
