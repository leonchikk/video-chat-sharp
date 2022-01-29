namespace VideoChat.Core.Models.EventArgs
{
    public class NewFrameEventArgs
    {
        public  NewFrameEventArgs(byte[] frame)
        {
            Frame = frame;
        }

        public byte[] Frame { get; private set; }
    }
}
