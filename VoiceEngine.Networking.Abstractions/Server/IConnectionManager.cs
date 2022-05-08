using System;
using System.Collections.Generic;
using VoiceEngine.Network.Abstractions.Server.Models;

namespace VoiceEngine.Network.Abstractions.Server
{
    public interface IConnectionManager
    {
        event Action<Connection> OnConnect;
        event Action<Connection> OnDisconnect;

        void Add(Connection connection);
        void Remove(string accountId);
        Connection Get(string accountId);
        IEnumerable<Connection> Get();

        int Online { get; }
    }
}
