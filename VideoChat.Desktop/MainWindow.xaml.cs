using Multimedia.Audio.Desktop;
using Multimedia.Video.Desktop;
using Multimedia.Video.Desktop.Codecs;
using Networking;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Helpers;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Multimedia.Codecs;
using VideoChat.Core.Networking;
using VideoChat.Core.Packets;
using VideoChat.Desktop.ViewModels;

namespace VideoChat.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly System.Timers.Timer _fpsTimer;
        private int _frameCount = 0;
        private int _incomingFrameCount = 0;

        public DeviceCapabilityViewModel DeviceCapabilityViewModel;
        public VideoDeviceViewModel VideoDeviceViewModel;

        public IVideoDeviceManager VideoDeviceManager;
        public IVideoEncoder VideoEncoder;
        public IVideoDecoder VideoDecoder;
        public IInputAudioDevice InputAudioDevice;
        public IOutputAudioDevice OutputAudioDevice;

        private IWebSocketClient _webSocketClient;
        private IHttpClientWrapper _httpClientWrapper;

        public MainWindow()
        {
            _fpsTimer = new System.Timers.Timer(1000);

            _fpsTimer.Elapsed += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_frameCount > 0)
                    {
                        FPS.Content = _frameCount;
                        _frameCount = 0;
                    }

                    if (_incomingFrameCount > 0)
                    {
                        FPS_Incoming.Content = _incomingFrameCount;
                        _incomingFrameCount = 0;
                    }
                }));
            };
            _fpsTimer.Start();

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
                case PacketTypeEnum.Video:

                    VideoDecoder?.Decode(e.PacketPayload);
                    break;

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
            VideoDeviceManager = new VideoDeviceManager();
            VideoEncoder = new VideoEncoder();
            VideoDecoder = new VideoDecoder();

            if (VideoDeviceManager.CurrentDevice.CurrentOption != null)
            {
                VideoEncoder.Setup(VideoDeviceManager.CurrentDevice.CurrentOption);
                DeviceCapabilityViewModel = new DeviceCapabilityViewModel(VideoDeviceManager.CurrentDevice.DeviceOptions, VideoDeviceManager.CurrentDevice.CurrentOption);
                DeviceCapabilitiesList.DataContext = DeviceCapabilityViewModel;
                VideoDeviceViewModel = new VideoDeviceViewModel(VideoDeviceManager.AvailableDevices, VideoDeviceManager.CurrentDevice?.Info);
                VideoDevicesList.DataContext = VideoDeviceViewModel;

                VideoDecoder.OnDecode += VideoDecoder_OnDecode;
                VideoDeviceManager.CurrentDevice.OnFrame += VideoDevice_OnFrame;
                VideoEncoder.OnEncode += VideoCodec_OnEncode;
            }
            InputAudioDevice.OnSampleRecorded += InputAudioDevice_OnSampleRecorded;
            OutputAudioDevice.Start();
        }

        private void VideoDecoder_OnDecode(Bitmap decodedImage)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _incomingFrameCount++;
                VideoField.Source = decodedImage.ToBitmapImage();
            }));
        }

        private void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            _webSocketClient.SendPacket(new Packet()
            {
                Type = PacketTypeEnum.Audio,
                PayloadBuffer = e.Buffer
            });
        }

        private void VideoCodec_OnEncode(byte[] buffer)
        {
            _webSocketClient.SendPacket(new Packet()
            {
                Type = PacketTypeEnum.Video,
                PayloadBuffer = buffer
            });
        }

        private void VideoDevice_OnFrame(Bitmap bitmap)
        {
            _frameCount++;

            VideoEncoder?.Encode(bitmap);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _webSocketClient.Disconnect();
            _webSocketClient.Dispose();
            _httpClientWrapper.Dispose();
            VideoDeviceManager.CurrentDevice?.Stop();
            InputAudioDevice.Stop();
            OutputAudioDevice.Stop();
            _fpsTimer.Start();
        }

        private void MicroOnButton_Click(object sender, RoutedEventArgs e)
        {
            InputAudioDevice.Start();
        }

        private void MicroOffButton_Click(object sender, RoutedEventArgs e)
        {
            InputAudioDevice.Stop();
        }

        private void CameraOnButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDeviceManager.CurrentDevice?.Start();
        }

        private void CameraOffButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDeviceManager.CurrentDevice?.Stop();
        }

        private void DeviceCapabilitiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var current = DeviceCapabilityViewModel.CurrentCapability;

            VideoDeviceManager.CurrentDevice?.Stop();
            VideoDeviceManager.CurrentDevice?.SetOption(current);
            VideoEncoder?.Setup(current);
        }

        private void VideoDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var current = VideoDeviceViewModel.CurrentDevice;

            VideoDeviceManager.CurrentDevice?.SwitchTo(current);

            DeviceCapabilityViewModel = new DeviceCapabilityViewModel(VideoDeviceManager.CurrentDevice?.DeviceOptions, VideoDeviceManager.CurrentDevice?.CurrentOption);
        }
    }
}
