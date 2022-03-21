using VideoChat.Core.Enumerations;

namespace VideoChat.Core.Packets
{
    public class Packet
    {
        public Packet(PacketTypeEnum type, byte[] payloadBuffer)
        {
            Type = type;
            PayloadBuffer = payloadBuffer;
        }

        public PacketTypeEnum Type { get; set; }
        public byte[] PayloadBuffer { get; set; }
    }
}
