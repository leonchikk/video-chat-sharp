using System;
using System.Threading.Tasks;
using VideoChat.Core.Models;
using VideoChat.Core.Packets;

namespace VideoChat.Core.Networking
{
    public interface IWebSocketClient: IDisposable
    {
        event Action<NetworkMessageReceivedEventArgs> OnMessage;
        event Action OnConnection;
        event Action OnDisconnect;

        void SendPacket(Packet packet);

        Task Connect(string jwtToken);
        Task Disconnect();
    }
}
