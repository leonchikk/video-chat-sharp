using Multimedia.Audio.Desktop;
using Networking;
using System;
using System.Windows;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Networking;
using VideoChat.Core.Packets;

namespace VoiceChat.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IInputAudioDevice InputAudioDevice;
        public IOutputAudioDevice OutputAudioDevice;

        private IWebSocketClient _webSocketClient;
        private IHttpClientWrapper _httpClientWrapper;

        public MainWindow()
        {
            _httpClientWrapper = new HttpClientWrapper();
            _webSocketClient = new WebSocketClient();

            var token = _httpClientWrapper.GetAuthorizationToken().ConfigureAwait(false).GetAwaiter().GetResult();

            _webSocketClient.Connect(token).ConfigureAwait(false).GetAwaiter().GetResult();
            _webSocketClient.OnMessage += WebSocketClient_OnMessage;

            InitializeComponent();
            ConfigWebCams();
        }

        private void WebSocketClient_OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.Audio:
                    OutputAudioDevice?.PlaySamples(e.PacketPayload, 60);
                    break;
                default:
                    break;
            }
        }

        public void ConfigWebCams()
        {
            InputAudioDevice = new InputAudioDevice();
            OutputAudioDevice = new OutputAudioDevice();

            InputAudioDevice.OnSampleRecorded += InputAudioDevice_OnSampleRecorded;
            OutputAudioDevice.Start();
        }
        private void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            _webSocketClient.SendPacket(new Packet()
            {
                Type = PacketTypeEnum.Audio,
                PayloadBuffer = e.Buffer
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _webSocketClient.Disconnect();
            _webSocketClient.Dispose();
            _httpClientWrapper.Dispose();
            InputAudioDevice.Stop();
            OutputAudioDevice.Stop();
        }

        private void MicroOnButton_Click(object sender, RoutedEventArgs e)
        {
            InputAudioDevice.Start();
        }

        private void MicroOffButton_Click(object sender, RoutedEventArgs e)
        {
            InputAudioDevice.Stop();
        }
    }
}
