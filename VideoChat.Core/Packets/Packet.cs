using System.IO;
using VideoChat.Core.Enumerations;

namespace VideoChat.Core.Packets
{
    public class Packet
    {
        public PacketTypeEnum Type { get; set; }
        public MemoryStream PayloadStream { get; set; }
    }
}
