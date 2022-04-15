namespace VoiceEngine.Abstractions.EventArgs
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
