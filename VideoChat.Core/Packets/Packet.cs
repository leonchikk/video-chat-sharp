namespace VideoChat.Core.Packets
{
    public abstract class Packet
    {
        public abstract byte[] Payload();
    }
}
