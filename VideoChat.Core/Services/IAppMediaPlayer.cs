using VoiceEngine.Abstractions.Enumerations;

namespace VoiceEngine.Abstractions.Services
{
    public interface IAppMediaPlayer
    {
        void Play(AppSoundEnum sound);
    }
}
