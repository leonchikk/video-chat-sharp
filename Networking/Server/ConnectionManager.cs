using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VoiceEngine.Network.Abstractions.Server;
using VoiceEngine.Network.Abstractions.Server.Models;

namespace VoiceEngine.Network.Server
{
    public class ConnectionManager : IConnectionManager
    {
        private ConcurrentDictionary<string, Connection> _connections;

        public ConnectionManager()
        {
            _connections = new ConcurrentDictionary<string, Connection>();
        }

        public int Online => _connections.Skip(0).Count();

        public void Add(Connection connection)
        {
            _connections.AddOrUpdate(connection.AccountId, connection, (key, oldValue) => connection);
        }

        public Connection Get(string accountId)
        {
            _connections.TryGetValue(accountId, out var connection);

            return connection;
        }

        public IEnumerable<Connection> Get()
        {
            return _connections.Values;
        }

        public void Remove(string accountId)
        {
            _connections.TryRemove(accountId, out var connection);
        }
    }
}
