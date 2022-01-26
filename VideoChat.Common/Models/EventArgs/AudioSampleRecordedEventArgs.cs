namespace VideoChat.Core.Models
{
    public class AudioSampleRecordedEventArgs
    {
        public byte[] Buffer { get; set; }
        public int Bytes { get; set; }
    }
}
