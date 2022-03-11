using VideoChat.Core.Enumerations;

namespace VideoChat.Core.Models
{
    public class NetworkMessageReceivedEventArgs
    {
        public NetworkMessageReceivedEventArgs(PacketTypeEnum packetTypeEnum, byte[] packetPayload)
        {
            PacketType = packetTypeEnum;
            PacketPayload = packetPayload;
        }

        public PacketTypeEnum PacketType { get; private set; }
        public byte[] PacketPayload { get; private set; }
    }
}
