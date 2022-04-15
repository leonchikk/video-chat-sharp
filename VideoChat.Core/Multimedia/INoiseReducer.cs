namespace VoiceEngine.Abstractions.Multimedia
{
    public interface INoiseReducer
    {
        void ReduceNoise(short[] pcm, int channel);
    }
}
