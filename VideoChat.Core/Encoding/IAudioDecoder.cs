namespace VoiceEngine.Abstractions.Encoding
{
    public interface IAudioDecoder
    {
        int Decode(byte[] buffer, int bufferLength, short[] pcm, bool fec = false);
    }
}
