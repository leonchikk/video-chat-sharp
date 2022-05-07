using System.IO;

namespace VoiceEngine.Network.Abstractions.Packets
{
    public class AudioPacket: Packet
    {
        public AudioPacket(bool containsSpeech, byte[] samples, string senderId)
        {
            ContainsSpeech = containsSpeech;
            Samples = samples;
            SenderId = senderId;
        }

        public bool ContainsSpeech { get; set; }
        public byte[] Samples { get; set; }
        public string SenderId { get; set; }

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
                    binaryWriter.Write(SenderId);

                    return stream.ToArray();
                }
            }
        }
    }
}

namespace VoiceEngine.Network.Abstractions.Packets.Convertor
{
    public static partial class PacketConvertor
    {
        public static AudioPacket ToAudioPacket(byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var containsSpeech = binaryReader.ReadBoolean();
                    var samplesCount = binaryReader.ReadInt32();
                    var samples = binaryReader.ReadBytes(samplesCount);
                    var senderId = binaryReader.ReadString();

                    return new AudioPacket(containsSpeech, samples, senderId);
                }
            }
        }
    }
}