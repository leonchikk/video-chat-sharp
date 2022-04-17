using VoiceEngine.Network.Abstractions.Enumerations;

namespace VoiceEngine.Network.Abstractions.Packets.Abstractions
{
    public abstract class EventPacket : Packet
    {
        public EventTypeEnum EventType { get; set; }
    }
}
