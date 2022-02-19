using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VideoChat.API.Sockets
{
    public class SocketHandler
    {
        private readonly ConnectionManager _connectionManager;

        public SocketHandler(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task HandleIncomings(WebSocket socket)
        {
            List<byte> receivedBytes = new List<byte>();
            var accountId = _connectionManager.GetAccountId(socket);

            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] tempStorage = new byte[2]; // 1016 is one chunck
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(tempStorage), cancellationToken: CancellationToken.None);

                    receivedBytes.AddRange(tempStorage);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[receivedBytes.Count];
                        for (int i = 0; i < buffer.Length; i++)
                            buffer[i] = receivedBytes[i];

                        receivedBytes.Clear();

                        //Send received data to other users
                        HandleRequest(accountId, buffer, socket);

                    }
                }
                catch (Exception ex)
                {
                    await _connectionManager.CloseConnection(socket);
                }
            }

            await _connectionManager.CloseConnection(accountId);
        }

        private async Task SendAsync(WebSocket socket, ArraySegment<byte> buffer, WebSocketMessageType socketMessageType = WebSocketMessageType.Text)
        {
            if (socket == null || socket.State != WebSocketState.Open)
                return;

            try
            {
                await socket.SendAsync(buffer: buffer,
                       messageType: socketMessageType,
                       endOfMessage: true,
                       cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                var id = _connectionManager.GetAccountId(socket);
                await _connectionManager.CloseConnection(id);
            }
        }

        private void HandleRequest(string accountId, byte[] buffer, WebSocket webSocket)
        {
            var otherRecipentSockets = _connectionManager.Connections
                .Where(x => x.Key != accountId)
                .Select(x => x.Value);

            foreach (var recipientSocket in otherRecipentSockets)
            {
               _ = SendAsync(recipientSocket, buffer, WebSocketMessageType.Binary);
            }
        }
    }
}
