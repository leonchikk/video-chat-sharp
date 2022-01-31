using System.IO;
using VideoChat.Core.Enumerations;

namespace VideoChat.Core.Packets
{
    public class Packet
    {
        public PacketTypeEnum Type { get; set; }
        public byte[] PayloadBuffer { get; set; }
    }
}
