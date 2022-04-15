using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceEngine.API.Sockets
{
    public class ConnectionsManager
    {
        public ConcurrentDictionary<string, WebSocket> UsersConnections { get; private set; }

        public ConnectionsManager()
        {
            UsersConnections = new ConcurrentDictionary<string, WebSocket>();
        }

        public WebSocket GetSocket(string id)
        {
            UsersConnections.TryGetValue(id, out var socket);

            return socket;
        }

        public string GetAccountId(WebSocket socket)
        {
            var result = UsersConnections.FirstOrDefault(x => x.Value == socket);
            return result.Key;
        }

        public void AddConnection(string accountId, WebSocket socket)
        {
            UsersConnections.TryAdd(accountId, socket);
        }

        public async Task CloseConnection(WebSocket socket)
        {
            var id = GetAccountId(socket);

            await CloseConnection(id);
        }

        public async Task CloseConnection(string id)
        {
            UsersConnections.TryRemove(id, out var socket);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
    }
}
