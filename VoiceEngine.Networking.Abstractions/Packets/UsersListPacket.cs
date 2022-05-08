using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace VoiceEngine.Network.Abstractions.Packets
{
    public class UsersListPacket : Packet
    {
        public string[] Ids { get; set; }

        public UsersListPacket(string[] ids)
        {
            Ids = ids;
        }

        public override byte[] Payload()
        {
            var json = JsonConvert.SerializeObject(Ids);

            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.UserList);
                    binaryWriter.Write(Encoding.ASCII.GetBytes(json));

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
        public static UsersListPacket ToUserListPacket(byte[] payload)
        {
            var json = Encoding.ASCII.GetString(payload);

            return new UsersListPacket(JsonConvert.DeserializeObject<string[]>(json));
        }
    }
}