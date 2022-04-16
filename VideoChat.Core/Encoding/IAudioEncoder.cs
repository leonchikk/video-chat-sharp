namespace VoiceEngine.Abstractions.Encoding
{
    public interface IAudioEncoder
    {
        int Encode(short[] pcm, byte[] buffer);
    }
}
