namespace VideoChat.Core.Models
{
    public class AudioSampleRecordedEventArgs
    {
        public AudioSampleRecordedEventArgs(byte[] buffer, int bytes)
        {
            Buffer = buffer;
            Bytes = bytes;
        }

        public byte[] Buffer { get; private set; }
        public int Bytes { get; private set; }
    }
}
