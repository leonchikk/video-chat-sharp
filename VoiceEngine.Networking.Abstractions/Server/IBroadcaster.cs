using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.Packets;

namespace VoiceEngine.Network.Abstractions.Server
{
    public interface IBroadcaster
    {
        Task ToUser(string userAccountId, Packet packet);
        Task ToUser(string userAccountId, byte[] packet);

        Task ToChannel(string channelId, Packet packet);
        Task ToChannel(string channelId, byte[] packet);

        Task ToAllExcept(string exceptUserAccountId, Packet packet);
        Task ToAllExcept(string exceptUserAccountId, byte[] packet);
    }
}
