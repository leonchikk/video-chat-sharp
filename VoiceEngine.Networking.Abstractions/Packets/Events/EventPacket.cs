using System.IO;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.Packets.Events;

namespace VoiceEngine.Network.Abstractions.Packets.Events
{
    public class EventPacket : Packet
    {
        public EventTypeEnum EventType { get; set; }
        public byte[] PacketPayload { get; set; }

        public EventPacket() { }

        public EventPacket(EventTypeEnum eventType, byte[] packetPayload)
        {
            EventType = eventType;
            PacketPayload = packetPayload;
        }

        public override byte[] Payload()
        {
            using (var stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.Event);
                    binaryWriter.Write(PacketPayload);

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
        public static EventPacket ToEventPacket(byte[] payload)
        {
            using (var memoryStream = new MemoryStream(payload))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var eventType = (EventTypeEnum)binaryReader.ReadByte();
                    var packetPayload = binaryReader.ReadBytes(payload.Length - 1);

                    return new EventPacket(eventType, packetPayload);
                }
            }
        }
    }
}
