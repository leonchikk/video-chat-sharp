using Networking.Factories;
using System;
using System.Windows;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Networking;
using VideoChat.Core.Packets;
using System.Linq;
using VideoChat.Core.Codec;
using System.Runtime.InteropServices;
using SpeexEchoReducer;
using SpeexPreprocessor;

namespace VoiceChat.Desktop
{
    public partial class MainWindow : Window
    {
        private IInputAudioDevice _inputAudioDevice;
        private IOutputAudioDevice _outputAudioDevice;
        private IWebSocketClient _webSocketClient;
        private IHttpClientWrapper _httpClientWrapper;
        private IAudioEncoder _encoder;
        private INoiseReducer _noiseReducer;

        private EchoReducer _echoReducer;
        private Preprocessor _preprocessor;

        private readonly byte[] _encodedBuffer = new byte[1024];

        public MainWindow(
            IInputAudioDevice inputAudioDevice,
            IOutputAudioDevice outputAudioDevice,
            IWebSocketClient webSocketClient,
            IHttpClientWrapper httpClientWrapper,
            INoiseReducer noiseReducer, 
            IAudioEncoder encoder)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _webSocketClient = webSocketClient;
            _httpClientWrapper = httpClientWrapper;
            _encoder = encoder;
            _noiseReducer = noiseReducer;

            _echoReducer = new EchoReducer(480, 48000);
            _preprocessor = new Preprocessor(480, 48000);

            _preprocessor.Denoise = false;
            _preprocessor.Dereverb = true;
            _preprocessor.Agc = true;
            _preprocessor.AgcLevel = 2000;
            _preprocessor.AgcMaxGain = 30;
            _preprocessor.AgcIncrement = 5;
            _preprocessor.AgcDecrement = -5;

            _webSocketClient.OnMessage += WebSocketClient_OnMessage;
            _inputAudioDevice.OnSamplesRecorded += InputAudioDevice_OnSampleRecorded;

            InitializeComponent();
        }

        private void WebSocketClient_OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.Audio:
                    var audioPacket = e.PacketPayload.ToAudioPacket();

                    //_echoReducer.EchoPlayback(audioPacket.Samples);
                    _outputAudioDevice?.PlaySamples(audioPacket.Samples, audioPacket.Samples.Length, audioPacket.ContainsSpeech);

                    break;
                default:
                    break;
            }
        }

        private async void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            var pcmInput = MemoryMarshal.Cast<byte, short>(e.Buffer).ToArray();
            var output_frame = e.Buffer;

            //_echoReducer.EchoCapture(pcmInput, output_frame);
            _preprocessor.Run(output_frame);

            var pcmOutput = MemoryMarshal.Cast<byte, short>(output_frame).ToArray();

            _noiseReducer.ReduceNoise(pcmOutput, 0);

            var encodedLength = _encoder.Encode(pcmOutput, _encodedBuffer);
            var encoded = new byte[encodedLength];

            Array.Copy(_encodedBuffer, encoded, encodedLength);

            await _webSocketClient.SendPacket(new AudioPacket(e.ContainsSpeech, encoded));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _webSocketClient.Disconnect();
            _webSocketClient.Dispose();
            _httpClientWrapper.Dispose();
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
            var token = await _httpClientWrapper.GetAuthorizationToken();

            await _webSocketClient.Connect(token);

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
    }
}
