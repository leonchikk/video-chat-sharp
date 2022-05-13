using System;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;

namespace VoiceEngine.Network.Abstractions.Server
{
    public interface ISocket: IDisposable
    {
        event Action<NetworkMessageReceivedEventArgs> OnMessage;
        event Action OnDisconnect;

        Task Send(Packet packet);
        Task Send(byte[] packet);
        Task Close();
        Task HandleIncomings();
    }
}
