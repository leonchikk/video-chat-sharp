using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceEngine.API.Sockets
{
    public class SocketHandler
    {
        private readonly ConnectionsManager _connectionManager;
        private byte[] _receivedBuffer = new byte[4096];

        public SocketHandler(ConnectionsManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task HandleIncomings(WebSocket socket)
        {
            var accountId = _connectionManager.GetAccountId(socket);

            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(_receivedBuffer), cancellationToken: CancellationToken.None);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[result.Count];
                        Buffer.BlockCopy(_receivedBuffer, 0, buffer, 0, result.Count);

                        //Send received data to other users
                        _ = HandleRequest(accountId, buffer, socket);
                    }
                }
                catch (Exception ex)
                {
                    _ = _connectionManager.CloseConnection(socket);
                }
            }

            _ = _connectionManager.CloseConnection(accountId);
        }

        private async Task SendAsync(WebSocket socket, ArraySegment<byte> buffer, CancellationToken cancellationToken, WebSocketMessageType socketMessageType = WebSocketMessageType.Text)
        {
            if (socket == null || socket.State != WebSocketState.Open)
                return;

            try
            {
                _ = socket.SendAsync(buffer: buffer,
                       messageType: socketMessageType,
                       endOfMessage: true,
                       cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var id = _connectionManager.GetAccountId(socket);
                _ = _connectionManager.CloseConnection(id);
            }
        }

        private async Task HandleRequest(string accountId, byte[] buffer, WebSocket webSocket)
        {
            var otherRecipentSockets = _connectionManager.UsersConnections
                .Where(x => x.Key != accountId)
                .Select(x => x.Value);

            foreach (var recipientSocket in otherRecipentSockets)
            {
                _ = SendAsync(recipientSocket, buffer, CancellationToken.None, WebSocketMessageType.Binary);
            }
        }
    }
}
