namespace VideoChat.Core.Codec
{
    public interface IAudioEncoder
    {
        int Encode(short[] pcm, byte[] buffer);
    }
}
