using System;
using System.Threading.Tasks;
using VideoChat.Core.Models;

namespace VideoChat.Core.Networking
{
    public interface IWebSocketClient
    {
        event Func<NetworkMessageReceivedEventArgs> OnMessageReceived;
        event Action OnOpenConnection;
        event Action OnCloseConnection;

        Task Connect();
        Task Disconnect();
    }
}
