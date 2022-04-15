namespace VoiceEngine.Abstractions.Filters
{
    public interface INoiseReducer
    {
        void ReduceNoise(short[] pcm, int channel);
    }
}
