using Newtonsoft.Json;
using System.IO;
using System.Text;
using VoiceEngine.Network.Abstractions.Models;

namespace VoiceEngine.Network.Abstractions.Packets
{
    public class UsersListPacket : Packet
    {
        public UserInfoModel[] Users { get; set; }

        public UsersListPacket(UserInfoModel[] users)
        {
            Users = users;
        }

        public override byte[] Payload()
        {
            var json = JsonConvert.SerializeObject(Users);

            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.UserList);
                    binaryWriter.Write(Encoding.UTF8.GetBytes(json));

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
            var json = Encoding.UTF8.GetString(payload);

            return new UsersListPacket(JsonConvert.DeserializeObject<UserInfoModel[]>(json));
        }
    }
}