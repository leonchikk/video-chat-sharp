using System;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;

namespace VoiceEngine.Network.Abstractions.Clients
{
    public interface IWebSocketClient: IDisposable
    {
        event Action<NetworkMessageReceivedEventArgs> OnMessage;
        event Action OnConnection;
        event Action OnDisconnect;

        Task SendPacket(Packet packet);

        Task Connect(string jwtToken);
        Task Disconnect();
    }
}
