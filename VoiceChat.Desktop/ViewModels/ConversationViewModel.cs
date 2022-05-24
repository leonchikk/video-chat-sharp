using SpeexPreprocessor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Threading;
using VoiceChat.Desktop.Stores;
using VoiceEngine.Abstractions.Encoding;
using VoiceEngine.Abstractions.EventArgs;
using VoiceEngine.Abstractions.Filters;
using VoiceEngine.Abstractions.IO;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;

namespace VoiceChat.Desktop.ViewModels
{
    public class ConversationViewModel : ViewModelBase
    {
        private readonly ISocketClient _socketClient;
        private readonly IInputAudioDevice _inputAudioDevice;
        private readonly IOutputAudioDevice _outputAudioDevice;
        private readonly INoiseReducer _noiseReducer;
        private readonly IAudioEncoder _encoder;
        private readonly IAudioDecoder _decoder;

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
            IAudioDecoder audioDecoder)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _socketClient = socketClient;
            _encoder = encoder;
            _noiseReducer = noiseReducer;
            _decoder = audioDecoder;

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

                    UserInfoViewModel = new UserInfoViewModel()
                    {
                        AccountId = UserStore.AccountId,
                        Nickname = UserStore.Nickname
                    };

                    //TODO: Move this out of view model (Into separate service and just inject interface here)
                    var uri = new Uri(@"Resources/Sounds/joined.mp3", UriKind.RelativeOrAbsolute);
                    var player = new MediaPlayer();

                    player.Open(uri);
                    player.Play();
                    //////////////////////////
                    ////(Another (crutch)
                    _inputAudioDevice.Start();
                    _outputAudioDevice.Start();
                    ////

                    foreach (var user in userListPacket.Users)
                    {

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ConnectionsList.Add(new ConnectionViewModel()
                            {
                                AccountId = user.AccountId,
                                Nickname = user.Nickname
                            });
                        });

                        _outputAudioDevice.AddInput(user.AccountId);
                    }

                    break;

                case PacketTypeEnum.Event:

                    var eventPacket = PacketConvertor.ToEventPacket(e.PacketPayload);

                    switch (eventPacket.EventType)
                    {
                        case EventTypeEnum.UserConnection:

                            var connectionPacket = PacketConvertor.ToUserConnectionPacket(eventPacket.PacketPayload);

                            _outputAudioDevice.AddInput(connectionPacket.AccountId);

                            //TODO: Move to extensions
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                ConnectionsList.Add(new ConnectionViewModel()
                                {
                                    AccountId = connectionPacket.AccountId,
                                    Nickname = connectionPacket.Nickname
                                });
                            });

                            //TODO: Move this out of view model (Into separate service and just inject interface here)
                            uri = new Uri(@"Resources/Sounds/joined.mp3", UriKind.RelativeOrAbsolute);
                            player = new MediaPlayer();

                            player.Open(uri);
                            player.Play();
                            ///////////////////////////
                            ///
                            break;

                        case EventTypeEnum.UserDisconnect:

                            var disconnectionPacket = PacketConvertor.ToUserDisconnectPacket(eventPacket.PacketPayload);

                            //TODO: Move to extensions
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                var toRemove = ConnectionsList.FirstOrDefault(x => x.AccountId == disconnectionPacket.AccountId);
                                ConnectionsList.Remove(toRemove);
                            });

                            //TODO: Move this out of view model (Into separate service and just inject interface here)
                            uri = new Uri(@"Resources/Sounds/left.mp3", UriKind.RelativeOrAbsolute);
                            player = new MediaPlayer();

                            player.Open(uri);
                            player.Play();
                            //////////////////////

                            _outputAudioDevice.RemoveInput(disconnectionPacket.AccountId);

                            break;
                        default:
                            break;
                    }

                    break;

                default:
                    break;
            }
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
