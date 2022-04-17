using System.IO;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.Packets.Abstractions;

namespace VoiceEngine.Network.Abstractions.Packets.Events
{
    public class UserConnectionPacket : EventPacket
    {
        public string AccountId { get; set; }

        public UserConnectionPacket(string accountId)
        {
            AccountId = accountId;
            EventType = EventTypeEnum.UserConnection;
        }


        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.Event);
                    binaryWriter.Write((byte)EventTypeEnum.UserConnection);
                    binaryWriter.Write(AccountId);

                    return stream.ToArray();
                }
            }
        }
    }
}
