using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VideoChat.API.Sockets
{
    public class ConnectionManager
    {
        public ConcurrentDictionary<string, WebSocket> Connections { get; private set; }

        public ConnectionManager()
        {
            Connections = new ConcurrentDictionary<string, WebSocket>();
        }

        public WebSocket GetSocket(string id)
        {
            Connections.TryGetValue(id, out var socket);

            return socket;
        }

        public string GetAccountId(WebSocket socket)
        {
            var result = Connections.FirstOrDefault(x => x.Value == socket);
            return result.Key;
        }

        public void HandleConnetion(string accountId, WebSocket socket)
        {
            Connections.TryAdd(accountId, socket);
        }

        public async Task CloseConnection(WebSocket socket)
        {
            var id = GetAccountId(socket);

            await CloseConnection(id);
        }

        public async Task CloseConnection(string id)
        {
            Connections.TryRemove(id, out var socket);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
    }
}
