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

        public event Action<NetworkMessageReceivedEventArgs> OnMessage;
        public event Action OnConnection;
        public event Action OnDisconnect;

        public WebSocketClient()
        {
            _clientSocket = new ClientWebSocket();
        }

        public async Task Connect(string jwtToken)
        {
            _clientSocket.Options.SetRequestHeader("Authorization", jwtToken);

            await _clientSocket.ConnectAsync(new Uri(_url), CancellationToken.None).ConfigureAwait(false);

            _ = Task.Run(ReceivePacketsJob).ConfigureAwait(false);

            OnConnection?.Invoke();
        }

        public async Task Disconnect()
        {
            await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            OnDisconnect?.Invoke();
        }

        public async Task SendPacket(Packet packet)
        {
            try 
            { 
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
               
            }
        }

        private async Task ReceivePacketsJob()
        {
            byte[] receivedStorage = new byte[4096];

            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
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

        public void Dispose()
        {
            _clientSocket.Dispose();
        }
    }
}
