
namespace VoiceEngine.Network.Abstractions.EventArgs
{
    public class NetworkMessageReceivedEventArgs
    {
        public NetworkMessageReceivedEventArgs(PacketTypeEnum packetTypeEnum, byte[] packetPayload, byte[] rawPaylaod)
        {
            PacketType = packetTypeEnum;
            PacketPayload = packetPayload;
            RawPayload = rawPaylaod;
        }

        public PacketTypeEnum PacketType { get; private set; }
        public byte[] PacketPayload { get; private set; }
        public byte[] RawPayload { get; private set; }
    }
}
