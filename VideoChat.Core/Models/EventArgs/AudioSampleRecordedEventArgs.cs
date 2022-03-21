namespace VideoChat.Core.Models
{
    public class AudioSampleRecordedEventArgs
    {
        public AudioSampleRecordedEventArgs(byte[] buffer, int bytes, bool containsSpeech)
        {
            Buffer = buffer;
            Bytes = bytes;
            ContainsSpeech = containsSpeech;
        }

        public byte[] Buffer { get; private set; }
        public int Bytes { get; private set; }
        public bool ContainsSpeech { get; private set; }
    }
}
