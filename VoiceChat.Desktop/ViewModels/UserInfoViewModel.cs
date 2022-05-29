using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using VoiceChat.Desktop.Commands;
using VoiceEngine.Abstractions.Enumerations;
using VoiceEngine.Abstractions.IO;
using VoiceEngine.Abstractions.Services;

namespace VoiceChat.Desktop.ViewModels
{
    public class UserInfoViewModel : ViewModelBase
    {
        public string Nickname { get; set; }
        public string AccountId { get; set; }
        public char? FirstLetter => Nickname == null ? null : char.ToUpper(Nickname.First());

        private bool _isMicroOn;
        public bool IsMicroOn
        {
            get { return _isMicroOn; }
            set
            {
                _isMicroOn = value;

                if (_isMicroOn)
                {
                    _inputAudioDevice.Start();
                    _appMediaPlayer.Play(AppSoundEnum.Unmuted);
                }
                else
                {
                    _inputAudioDevice.Stop();
                    _appMediaPlayer.Play(AppSoundEnum.Muted);
                }

                OnPropertyChanged(nameof(IsMicroOn));
            }
        }

        public ICommand SwitchCommand { get; }

        private readonly IAppMediaPlayer _appMediaPlayer;
        private readonly IInputAudioDevice _inputAudioDevice;

        public UserInfoViewModel(IAppMediaPlayer appMediaPlayer, IInputAudioDevice inputAudioDevice)
        {
            _appMediaPlayer = appMediaPlayer;
            _inputAudioDevice = inputAudioDevice;

            SwitchCommand = new SwitchMicroStateCommand(this);
        }
    }
}
