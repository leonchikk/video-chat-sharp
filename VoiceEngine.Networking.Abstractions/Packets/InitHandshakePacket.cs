using System.IO;

namespace VoiceEngine.Network.Abstractions.Packets
{
    public class InitHandshakePacket : Packet
    {
        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.InitHandshake);

                    return stream.ToArray();
                }
            }
        }
    }
}
