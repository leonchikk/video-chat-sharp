using System;
using System.Collections.Generic;
using System.Windows.Media;
using VoiceEngine.Abstractions.Enumerations;
using VoiceEngine.Abstractions.Services;

namespace VoiceChat.Desktop.Services
{
    public class AppMediaPlayer : IAppMediaPlayer
    {
        private readonly Dictionary<AppSoundEnum, Uri> _appSounds;

        public AppMediaPlayer()
        {
            _appSounds = new Dictionary<AppSoundEnum, Uri>()
            {
                { AppSoundEnum.Joined, new Uri(@"Resources/Sounds/joined.mp3", UriKind.RelativeOrAbsolute) },
                { AppSoundEnum.Left, new Uri(@"Resources/Sounds/left.mp3", UriKind.RelativeOrAbsolute) },
                { AppSoundEnum.Muted, new Uri(@"Resources/Sounds/mute.mp3", UriKind.RelativeOrAbsolute) },
                { AppSoundEnum.Unmuted, new Uri(@"Resources/Sounds/unmute.mp3", UriKind.RelativeOrAbsolute) }
            };
        }

        public void Play(AppSoundEnum sound)
        {
            var mediaPlayer = new MediaPlayer();
            var soundToPlay = _appSounds[sound];

            mediaPlayer.Open(soundToPlay);
            mediaPlayer.Play();
        }
    }
}
