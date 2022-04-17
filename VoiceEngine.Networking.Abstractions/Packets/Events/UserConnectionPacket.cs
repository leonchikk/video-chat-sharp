﻿using System.IO;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.Packets.Events;

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
                    var accountId = binaryReader.ReadString();

                    return new UserConnectionPacket(accountId);
                }
            }
        }
    }
}