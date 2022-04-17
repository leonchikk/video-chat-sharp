using System.IO;
using VoiceEngine.Network.Abstractions.Packets.Events;

namespace VoiceEngine.Network.Abstractions.Extensions
{
    public static class UserConnectionPacketExtension
    {
        public static UserConnectionPacket ToUserConnectionPacket(this byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var accountId = binaryReader.ReadString();

                    return new UserConnectionPacket(accountId);
                }
            }
        }
    }
}
