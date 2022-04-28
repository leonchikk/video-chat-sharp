﻿using Microsoft.Win32;
using SpeexEchoReducer;
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
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Packets.Convertor;

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

        private IAudioEncoder _encoder;
        private IAudioDecoder _decoder;

        private Preprocessor _inputPreproccesor;
        private Preprocessor _outputPreproccesor;
        private EchoReducer _echoReducer;

        private readonly byte[] _encodedBuffer = new byte[1024];
        private short[] _pcmDecodedBuffer = new short[480];
        private byte[] _echoBuffer = new byte[960];
        private byte[] _inputBuffer = new byte[960];

        private bool _isAECEnabled;

        public MainWindow(
            IInputAudioDevice inputAudioDevice,
            IOutputAudioDevice outputAudioDevice,
            ISocketClient webSocketClient,
            IRestClient httpClientWrapper,
            INoiseReducer noiseReducer,
            IAudioEncoder encoder,
            IAudioRecorder audioRecorder,
            IAudioDecoder audioDecoder)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _socketClient = webSocketClient;
            _restClient = httpClientWrapper;
            _encoder = encoder;
            _noiseReducer = noiseReducer;
            _audioRecorder = audioRecorder;
            _decoder = audioDecoder;

            _inputPreproccesor = new Preprocessor(480, 48000);

            _inputPreproccesor.Denoise = true;
            _inputPreproccesor.Dereverb = true;
            _inputPreproccesor.Agc = true;
            _inputPreproccesor.AgcLevel = 8000;
            _inputPreproccesor.AgcMaxGain = 30;
            _inputPreproccesor.AgcIncrement = 12;
            _inputPreproccesor.AgcDecrement = -40;

            _outputPreproccesor = new Preprocessor(480, 48000);

            _outputPreproccesor.Denoise = true;
            _outputPreproccesor.Dereverb = false;
            _outputPreproccesor.Agc = true;
            _outputPreproccesor.AgcLevel = 2200;
            _outputPreproccesor.AgcMaxGain = 30;
            _outputPreproccesor.AgcIncrement = 12;
            _outputPreproccesor.AgcDecrement = -40;

            _echoReducer = new EchoReducer(480, 48000);

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

                    var decodedSamples = (MemoryMarshal.Cast<short, byte>(_pcmDecodedBuffer)).ToArray();

                    //if (_isAECEnabled)
                    //_echoReducer.EchoPlayback(decodedSamples);

                    //_outputPreproccesor.Run(decodedSamples);

                    //var pcmOutput = MemoryMarshal.Cast<byte, short>(decodedSamples).ToArray();
                    //_noiseReducer.ReduceNoise(pcmOutput, 0);

                    Buffer.BlockCopy(decodedSamples, 0, _echoBuffer, 0, decodedSamples.Length);

                    _outputAudioDevice?.PlaySamples(decodedSamples, decodedSamples.Length, audioPacket.ContainsSpeech);

                    if (_audioRecorder.IsRecording)
                    {
                        _audioRecorder.AddSamples(decodedSamples, decodedSamples.Length);
                    }

                    break;

                case PacketTypeEnum.Event:
                    var eventPacket = PacketConvertor.ToEventPacket(e.PacketPayload);

                    break;

                default:
                    break;
            }
        }

        private async void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            var buffer = e.Buffer;
            var outputBuffer = new byte[960];

            
            _echoReducer.EchoCancellation(buffer, _echoBuffer, outputBuffer);
            _inputPreproccesor.Run(buffer);

            Array.Copy(outputBuffer, _inputBuffer, e.Bytes);
            //var pcmInput = MemoryMarshal.Cast<byte, short>(e.Buffer).ToArray();
            //var output_frame = e.Buffer;
            //var pcmOutput = MemoryMarshal.Cast<byte, short>(output_frame).ToArray();
            var pcmOutput = MemoryMarshal.Cast<byte, short>(outputBuffer).ToArray();

            //_noiseReducer.ReduceNoise(pcmOutput, 0);
            //_preprocessor.EchoCancellation(pcmInput, _echoBuffer, output_frame);

            var encodedLength = _encoder.Encode(pcmOutput, _encodedBuffer);
            var encoded = new byte[encodedLength];

            Array.Copy(_encodedBuffer, encoded, encodedLength);

            await _socketClient.SendPacket(new AudioPacket(e.ContainsSpeech, encoded));
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

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isAECEnabled = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isAECEnabled = true;
        }
    }
}
