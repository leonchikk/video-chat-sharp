using System.IO;
using System.Text;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.Packets.Events;

namespace VoiceEngine.Network.Abstractions.Packets.Events
{
    public class UserConnectionPacket : EventPacket
    {
        public string AccountId { get; set; }
        public string Nickname { get; set; }

        public UserConnectionPacket(string accountId, string nickname)
        {
            AccountId = accountId;
            Nickname = nickname;
            EventType = EventTypeEnum.UserConnection;
        }


        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    var encoding = Encoding.UTF8;

                    binaryWriter.Write((byte)PacketTypeEnum.Event);
                    binaryWriter.Write((byte)EventTypeEnum.UserConnection);
                    binaryWriter.Write(encoding.GetByteCount(AccountId));
                    binaryWriter.Write(encoding.GetBytes(AccountId));
                    binaryWriter.Write(encoding.GetByteCount(Nickname));
                    binaryWriter.Write(encoding.GetBytes(Nickname));

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
        public static UserConnectionPacket ToUserConnectionPacket(byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var accountIdBytescount = binaryReader.ReadInt32();
                    var accountIdBytes = binaryReader.ReadBytes(accountIdBytescount);

                    var nicknameBytescount = binaryReader.ReadInt32();
                    var nicknameBytes = binaryReader.ReadBytes(nicknameBytescount);

                    return new UserConnectionPacket(Encoding.UTF8.GetString(accountIdBytes), Encoding.UTF8.GetString(nicknameBytes));
                }
            }
        }
    }
}