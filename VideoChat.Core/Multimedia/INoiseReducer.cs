namespace VideoChat.Core.Multimedia
{
    public interface INoiseReducer
    {
        void ReduceNoise(short[] pcm, int channel);
    }
}
