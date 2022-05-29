using System.IO;
using System.Text;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.Packets.Events;

namespace VoiceEngine.Network.Abstractions.Packets.Events
{
    public class MutePacket : Packet
    {
        public string AccountId { get; set; }

        public MutePacket(string accountId)
        {
            AccountId = accountId;
        }

        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    var encoding = Encoding.UTF8;

                    binaryWriter.Write((byte)PacketTypeEnum.Event);
                    binaryWriter.Write((byte)EventTypeEnum.Mute);
                    binaryWriter.Write(encoding.GetByteCount(AccountId));
                    binaryWriter.Write(encoding.GetBytes(AccountId));

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
        public static MutePacket ToMutePacket(byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var accountBytesCount = binaryReader.ReadInt32();
                    var accountId = Encoding.UTF8.GetString(binaryReader.ReadBytes(accountBytesCount));

                    return new MutePacket(accountId);
                }
            }
        }
    }
}
