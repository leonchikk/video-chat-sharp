using Networking.Factories;
using System;
using System.Windows;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Networking;
using VideoChat.Core.Packets;

namespace VoiceChat.Desktop
{
    public partial class MainWindow : Window
    {
        private IInputAudioDevice _inputAudioDevice;
        private IOutputAudioDevice _outputAudioDevice;
        private IWebSocketClient _webSocketClient;
        private IHttpClientWrapper _httpClientWrapper;

        public MainWindow(
            IInputAudioDevice inputAudioDevice,
            IOutputAudioDevice outputAudioDevice,
            IWebSocketClient webSocketClient,
            IHttpClientWrapper httpClientWrapper)
        {
            _inputAudioDevice = inputAudioDevice;
            _outputAudioDevice = outputAudioDevice;
            _webSocketClient = webSocketClient;
            _httpClientWrapper = httpClientWrapper;

            _webSocketClient.OnMessage += WebSocketClient_OnMessage;
            _inputAudioDevice.OnSampleRecorded += InputAudioDevice_OnSampleRecorded;

            InitializeComponent();
        }

        private void WebSocketClient_OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.Audio:
                    var audioPacket = e.PacketPayload.ToAudioPacket();
                    _outputAudioDevice?.PlaySamples(audioPacket.Samples, audioPacket.ContainsSpeech);
                    break;
                default:
                    break;
            }
        }

        private void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            _webSocketClient.SendPacket(new AudioPacket(e.ContainsSpeech, e.Buffer));
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
        }
    }
}
