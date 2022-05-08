﻿using Microsoft.Win32;
using SpeexPreprocessor;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using VoiceEngine.Abstractions.Encoding;
using VoiceEngine.Abstractions.EventArgs;
using VoiceEngine.Abstractions.Filters;
using VoiceEngine.Abstractions.IO;
using VoiceEngine.Abstractions.Models;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.Enumerations;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;
using VoiceEngine.Network.Abstractions.Services;

namespace VoiceChat.Desktop
{
    public partial class MainWindow : Window
    {
        private IInputAudioDevice _inputAudioDevice;
        private IOutputAudioDevice _outputAudioDevice;
        private ISocketClient _socketClient;
        private IRestClient _restClient;
        private INoiseReducer _noiseReducer;
        private IAudioRecorder _audioRecorder;
        private ITokenService _tokenService;

        private IAudioEncoder _encoder;
        private IAudioDecoder _decoder;

        private Preprocessor _preprocessor;

        private readonly byte[] _encodedBuffer = new byte[1024];
        private short[] _pcmDecodedBuffer = new short[480];
        private string _accountId;

        public MainWindow(
            IInputAudioDevice inputAudioDevice,
            IOutputAudioDevice outputAudioDevice,
            ISocketClient webSocketClient,
            IRestClient httpClientWrapper,
            INoiseReducer noiseReducer,
            IAudioEncoder encoder,
            IAudioRecorder audioRecorder,
            IAudioDecoder audioDecoder,
            ITokenService tokenService)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _socketClient = webSocketClient;
            _restClient = httpClientWrapper;
            _encoder = encoder;
            _noiseReducer = noiseReducer;
            _audioRecorder = audioRecorder;
            _decoder = audioDecoder;
            _tokenService = tokenService;

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

            _socketClient.OnMessage += WebSocketClient_OnMessage;
            _inputAudioDevice.OnSamplesRecorded += InputAudioDevice_OnSampleRecorded;

            InitializeComponent();
        }

        private void WebSocketClient_OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.Audio:

                    var audioPacket =  PacketConvertor.ToAudioPacket(e.PacketPayload);

                    _decoder.Decode(audioPacket.Samples, audioPacket.Samples.Length, _pcmDecodedBuffer);
                    _preprocessor.Run(_pcmDecodedBuffer);
                    _noiseReducer.ReduceNoise(_pcmDecodedBuffer, 0);
                    _outputAudioDevice?.PlaySamples(audioPacket.SenderId, _pcmDecodedBuffer, _pcmDecodedBuffer.Length * sizeof(short));

                    if (_audioRecorder.IsRecording)
                    {
                        _audioRecorder.AddSamples(audioPacket.SenderId, _pcmDecodedBuffer, _pcmDecodedBuffer.Length * sizeof(short));
                    }

                    break;

                case PacketTypeEnum.UserList:

                    var userListPacket = PacketConvertor.ToUserListPacket(e.PacketPayload);

                    foreach (var userId in userListPacket.Ids)
                    {
                        _outputAudioDevice.AddInput(userId);

                        Dispatcher.Invoke(() =>
                        {
                            OthersIdLabel.Content = userId;
                        });
                    }

                    break;

                case PacketTypeEnum.Event:

                    var eventPacket = PacketConvertor.ToEventPacket(e.PacketPayload);

                    switch (eventPacket.EventType)
                    {
                        case EventTypeEnum.UserConnection:

                            var connectionPacket = PacketConvertor.ToUserConnectionPacket(eventPacket.PacketPayload);

                            _outputAudioDevice.AddInput(connectionPacket.AccountId);
                            _audioRecorder.AddInput(connectionPacket.AccountId);

                            Dispatcher.Invoke(() =>
                            {
                                OthersIdLabel.Content = connectionPacket.AccountId;
                            });

                            break;

                        case EventTypeEnum.UserDisconnect:

                            var disconnectionPacket = PacketConvertor.ToUserDisconnectPacket(eventPacket.PacketPayload);

                            _outputAudioDevice.RemoveInput(disconnectionPacket.AccountId);
                            _audioRecorder.RemoveInput(disconnectionPacket.AccountId);

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

            await _socketClient.SendPacket(new AudioPacket(e.ContainsSpeech, encoded, _accountId));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _socketClient.Disconnect();
            _socketClient.Dispose();
            _restClient.Dispose();
            _inputAudioDevice.Stop();
            _outputAudioDevice.Stop();
        }

        private void MicroOnButton_Click(object sender, RoutedEventArgs e)
        {
            _inputAudioDevice.Start();
        }

        private void MicroOffButton_Click(object sender, RoutedEventArgs e)
        {
            _inputAudioDevice.Stop();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var token = await _restClient.GetAuthorizationToken();

            _accountId = _tokenService.GetAccountId(token);

            MyIdLabel.Content = $"Id: {_accountId}";

            await _socketClient.Connect(token);

            _outputAudioDevice.Start();

            InputDeviceDropdown.ItemsSource = _inputAudioDevice.Options;
            OutputDeviceDropdown.ItemsSource = _outputAudioDevice.Options;
            InputDeviceDropdown.SelectedItem = _inputAudioDevice.SelectedOption;
            OutputDeviceDropdown.SelectedItem = _outputAudioDevice.SelectedOption;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _outputAudioDevice.ChangeVolume(Convert.ToSingle(e.NewValue));
        }

        private void InputDeviceDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedDevice = e.AddedItems.Count > 0 ? (e.AddedItems[0] as AudioDeviceOptions) : null;

            if (selectedDevice == null || selectedDevice == _inputAudioDevice.SelectedOption)
                return;

            _inputAudioDevice.SwitchTo(selectedDevice);
            _inputAudioDevice.Start();
        }

        private void OutputDeviceDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedDevice = e.AddedItems.Count > 0 ? (e.AddedItems[0] as AudioDeviceOptions) : null;

            if (selectedDevice == null || selectedDevice == _outputAudioDevice.SelectedOption)
                return;

            _outputAudioDevice.SwitchTo(selectedDevice);
            _outputAudioDevice.Start();
        }

        private void StartStopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_audioRecorder.IsRecording)
            {
                var saveFileDialog = new SaveFileDialog();

                if (saveFileDialog.ShowDialog() == true)
                {
                    _audioRecorder.Start(saveFileDialog.FileName);
                    RecordingLabel.Visibility = Visibility.Visible;
                }

                return;
            }

            RecordingLabel.Visibility = Visibility.Collapsed;

            _audioRecorder.Stop();
        }
    }
}
