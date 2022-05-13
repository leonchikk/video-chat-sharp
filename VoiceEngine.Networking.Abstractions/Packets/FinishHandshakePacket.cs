using System;
using System.IO;
using System.Text;

namespace VoiceEngine.Network.Abstractions.Packets
{
    public class FinishHandshakePacket : Packet
    {
        public string SenderId { get; set; }
        public string Nickname { get; set; }

        public FinishHandshakePacket(string senderId, string nickname)
        {
            SenderId = senderId;
            Nickname = nickname;
        }

        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    var encoding = Encoding.UTF8;
                    var nicknameBytes = encoding.GetBytes(Nickname);
                    var senderId = encoding.GetBytes(SenderId);

                    binaryWriter.Write((byte)PacketTypeEnum.FinishHandshake);
                    binaryWriter.Write(nicknameBytes.Length);
                    binaryWriter.Write(nicknameBytes);
                    binaryWriter.Write(senderId.Length);
                    binaryWriter.Write(senderId);

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
        public static FinishHandshakePacket ToFinishHandshakePacket(byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {

                    var nicknameBytesCount = binaryReader.ReadInt32();
                    var nicknameBytes = binaryReader.ReadBytes(nicknameBytesCount);

                    var senderIdBytesCount = binaryReader.ReadInt32();
                    var senderIdBytes = binaryReader.ReadBytes(senderIdBytesCount);

                    var encoding = Encoding.UTF8;

                    return new FinishHandshakePacket(encoding.GetString(senderIdBytes), encoding.GetString(nicknameBytes));
                }
            }
        }
    }
}
