using SpeexPreprocessor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using VoiceChat.Desktop.Stores;
using VoiceEngine.Abstractions.Encoding;
using VoiceEngine.Abstractions.Enumerations;
using VoiceEngine.Abstractions.EventArgs;
using VoiceEngine.Abstractions.Filters;
using VoiceEngine.Abstractions.IO;
using VoiceEngine.Abstractions.Services;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;
using VoiceEngine.Network.Abstractions.Packets.Events;

namespace VoiceChat.Desktop.ViewModels
{
    public class ConversationViewModel : ViewModelBase
    {
        public bool IsLoaded { get; set; }

        private readonly ISocketClient _socketClient;
        private readonly IInputAudioDevice _inputAudioDevice;
        private readonly IOutputAudioDevice _outputAudioDevice;
        private readonly INoiseReducer _noiseReducer;
        private readonly IAudioEncoder _encoder;
        private readonly IAudioDecoder _decoder;
        private readonly IAppMediaPlayer _appMediaPlayer;

        private readonly Preprocessor _preprocessor;

        private readonly byte[] _encodedBuffer = new byte[1024];
        private short[] _pcmDecodedBuffer = new short[480];

        private ObservableCollection<ConnectionViewModel> _connectionsList;
        public ObservableCollection<ConnectionViewModel> ConnectionsList
        {
            get { return _connectionsList; }
            set
            {
                _connectionsList = value;
                OnPropertyChanged("ConnectionsList");
            }
        }

        private UserInfoViewModel _userInfoViewModel;
        public UserInfoViewModel UserInfoViewModel
        {
            get { return _userInfoViewModel; }
            set
            {
                _userInfoViewModel = value;
                OnPropertyChanged("UserInfoViewModel");
            }
        }

        public ConversationViewModel(
            IInputAudioDevice inputAudioDevice,
            IOutputAudioDevice outputAudioDevice,
            ISocketClient socketClient,
            INoiseReducer noiseReducer,
            IAudioEncoder encoder,
            IAudioDecoder audioDecoder,
            IAppMediaPlayer appMediaPlayer)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _socketClient = socketClient;
            _encoder = encoder;
            _noiseReducer = noiseReducer;
            _decoder = audioDecoder;
            _appMediaPlayer = appMediaPlayer;

            _preprocessor = new Preprocessor(480, 48000)
            {
                Denoise = true,
                Dereverb = true,
                Agc = true,
                AgcLevel = 4000,
                AgcMaxGain = 3,
                AgcIncrement = 80,
                AgcDecrement = -80
            };

            _connectionsList = new ObservableCollection<ConnectionViewModel>();

            _inputAudioDevice.OnSamplesRecorded += InputAudioDevice_OnSampleRecorded;
            _socketClient.OnMessage += OnMessage;

            IsLoaded = false;
        }

        private void OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.Audio:

                    var audioPacket = PacketConvertor.ToAudioPacket(e.PacketPayload);

                    _decoder.Decode(audioPacket.Samples, audioPacket.Samples.Length, _pcmDecodedBuffer);
                    _preprocessor.Run(_pcmDecodedBuffer);
                    _noiseReducer.ReduceNoise(_pcmDecodedBuffer, 0);
                    _outputAudioDevice?.PlaySamples(audioPacket.SenderId, _pcmDecodedBuffer, _pcmDecodedBuffer.Length * sizeof(short));

                    break;

                case PacketTypeEnum.UserList:

                    var userListPacket = PacketConvertor.ToUserListPacket(e.PacketPayload);

                    UserInfoViewModel = new UserInfoViewModel(_appMediaPlayer, _inputAudioDevice)
                    {
                        AccountId = UserStore.AccountId,
                        Nickname = UserStore.Nickname,
                        IsMicroOn = true
                    };

                    UserInfoViewModel.PropertyChanged += UserInfoViewModel_PropertyChanged;

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var user in userListPacket.Users)
                        {
                            ConnectionsList.Add(new ConnectionViewModel(user.AccountId, user.Nickname));

                            _outputAudioDevice.AddInput(user.AccountId);
                        }
                    });

                    _outputAudioDevice.Start();

                    break;

                case PacketTypeEnum.Event:

                    if (!IsLoaded)
                        return;

                    var eventPacket = PacketConvertor.ToEventPacket(e.PacketPayload);

                    switch (eventPacket.EventType)
                    {
                        case EventTypeEnum.UserConnection:

                            var connectionPacket = PacketConvertor.ToUserConnectionPacket(eventPacket.PacketPayload);

                            _outputAudioDevice.AddInput(connectionPacket.AccountId);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                ConnectionsList.Add(new ConnectionViewModel(connectionPacket.AccountId, connectionPacket.Nickname));
                            });

                            _appMediaPlayer.Play(AppSoundEnum.Joined);

                            break;

                        case EventTypeEnum.UserDisconnect:

                            var disconnectionPacket = PacketConvertor.ToUserDisconnectPacket(eventPacket.PacketPayload);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                var toRemove = ConnectionsList.FirstOrDefault(x => x.AccountId == disconnectionPacket.AccountId);
                                ConnectionsList.Remove(toRemove);
                            });

                            _appMediaPlayer.Play(AppSoundEnum.Left);
                            _outputAudioDevice.RemoveInput(disconnectionPacket.AccountId);

                            break;

                        case EventTypeEnum.Mute:

                            var mutePacket = PacketConvertor.ToMutePacket(eventPacket.PacketPayload);
                            var user = ConnectionsList.FirstOrDefault(x => x.AccountId == mutePacket.AccountId);
                            user.IsMuted = true;

                            _appMediaPlayer.Play(AppSoundEnum.Muted);

                            break;

                        case EventTypeEnum.Unmute:

                            var unmutePacket = PacketConvertor.ToUnmutePacket(eventPacket.PacketPayload);
                            user = ConnectionsList.FirstOrDefault(x => x.AccountId == unmutePacket.AccountId);
                            user.IsMuted = false;

                            _appMediaPlayer.Play(AppSoundEnum.Unmuted);

                            break;

                        default:
                            break;
                    }

                    break;

                default:
                    break;
            }
        }

        private async void UserInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (UserInfoViewModel.IsMicroOn)
                await _socketClient.SendPacket(new UnmutePacket(UserStore.AccountId));
            else
                await _socketClient.SendPacket(new MutePacket(UserStore.AccountId));
        }

        private async void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            var microBuffer = MemoryMarshal.Cast<byte, short>(e.Buffer).ToArray();

            var encodedLength = _encoder.Encode(microBuffer, _encodedBuffer);
            var encoded = new byte[encodedLength];

            Array.Copy(_encodedBuffer, encoded, encodedLength);

            await _socketClient.SendPacket(new AudioPacket(e.ContainsSpeech, encoded, UserStore.AccountId));
        }

    }
}
