using System.IO;
using VideoChat.Core.Enumerations;

namespace VideoChat.Core.Packets
{
    public class AudioPacket: Packet
    {
        public AudioPacket(bool containsSpeech, byte[] samples)
        {
            ContainsSpeech = containsSpeech;
            Samples = samples;
        }

        public bool ContainsSpeech { get; set; }
        public byte[] Samples { get; set; }

        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.Audio);
                    binaryWriter.Write(ContainsSpeech);
                    binaryWriter.Write(Samples.Length);
                    binaryWriter.Write(Samples);

                    return stream.ToArray();
                }
            }
        }
    }
}
