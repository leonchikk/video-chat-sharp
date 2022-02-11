using Multimedia.Audio.Desktop;
using Multimedia.Video.Desktop;
using Multimedia.Video.Desktop.Codecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Multimedia.Codecs;
using VideoChat.Core.Packets;
using VideoChat.Desktop.ViewModels;

namespace VideoChat.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly ClientWebSocket _clientSocket;
        private readonly ConcurrentQueue<Packet> _concurrentQueue;

        public IVideoDevice VideoDevice;
        public IVideoEncoder VideoEncoder;
        public IVideoDecoder VideoDecoder;
        public IInputAudioDevice InputAudioDevice;
        public IOutputAudioDevice OutputAudioDevice;

        public event Action<Packet> OnDataReceive;

        public MainWindow()
        {
            _concurrentQueue = new ConcurrentQueue<Packet>();

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync("http://192.168.0.107:5000/api/auth", null)
                           .ConfigureAwait(false)
                           .GetAwaiter()
                           .GetResult();

                var jwtToken = response.Content.ReadAsStringAsync()
                           .ConfigureAwait(false)
                           .GetAwaiter()
                           .GetResult();

                _clientSocket = new ClientWebSocket();
                _clientSocket.Options.SetRequestHeader("Authorization", jwtToken);
                _clientSocket.ConnectAsync(new Uri("ws://192.168.0.107:5000"), CancellationToken.None)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                OnDataReceive += DataRecive;

                //REFACTOR ME
                Task.Run(Receive);
                Task.Run(ProccesQueue);

                //clientSocket.

            }

            InitializeComponent();
            ConfigWebCams();
        }

        private async Task Receive()
        {
            List<byte> receivedBytes = new List<byte>();

            while (_clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] tempStorage = new byte[2048]; // 1016 is one chunck
                    var result = await _clientSocket.ReceiveAsync(buffer: new ArraySegment<byte>(tempStorage), cancellationToken: CancellationToken.None);

                    receivedBytes.AddRange(tempStorage);

                    if (result.EndOfMessage)
                    {
                        byte[] buffer = new byte[receivedBytes.Count];
                        for (int i = 0; i < buffer.Length; i++)
                            buffer[i] = receivedBytes[i];

                        receivedBytes.Clear();

                        using (var stream = new MemoryStream(buffer))
                        using (var binaryReader = new BinaryReader(stream))
                        {
                            var packetTypeEnum = (PacketTypeEnum)binaryReader.ReadByte();
                            var payloadLength = binaryReader.ReadInt32();
                            var payload = binaryReader.ReadBytes(payloadLength);

                            var packet = new Packet()
                            {
                                PayloadBuffer = payload,
                                Type = packetTypeEnum
                            };

                            OnDataReceive?.Invoke(packet);
                        }
                    }

                    //await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }

        public void ConfigWebCams()
        {
            InputAudioDevice = new InputAudioDevice();
            OutputAudioDevice = new OutputAudioDevice();
            VideoDevice = new VideoDevice();
            VideoEncoder = new VideoEncoder();
            VideoDecoder = new VideoDecoder();

            if (VideoDevice.CurrentDeviceCapability != null)
            {
                VideoEncoder.Setup(VideoDevice.CurrentDeviceCapability);
            }

            VideoDevicesList.DataContext = new VideoDeviceViewModel(VideoDevice.AvailableDevices);

            VideoDecoder.OnDecode += VideoDecoder_OnDecode;
            VideoDevice.OnFrame += VideoDevice_OnFrame;
            VideoEncoder.OnEncode += VideoCodec_OnEncode;
            InputAudioDevice.OnSampleRecorded += InputAudioDevice_OnSampleRecorded;
            OutputAudioDevice.Start();
        }

        private void VideoDecoder_OnDecode(Bitmap decodedImage)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                VideoField.Source = ToBitmapImage(decodedImage);
            }));
        }

        private void InputAudioDevice_OnSampleRecorded(AudioSampleRecordedEventArgs e)
        {
            _concurrentQueue.Enqueue(new Packet()
            {
                Type = PacketTypeEnum.Audio,
                PayloadBuffer = e.Buffer
            });
        }

        private void VideoCodec_OnEncode(byte[] buffer)
        {
            _concurrentQueue.Enqueue(new Packet()
            {
                Type = PacketTypeEnum.Video,
                PayloadBuffer = buffer
            });
        }

        private void VideoDevice_OnFrame(Bitmap bitmap)
        {
            VideoEncoder?.Encode(bitmap);
        }

        private async Task ProccesQueue()
        {
            while (true)
            {
                _concurrentQueue.TryDequeue(out var packet);

                if (packet == null)
                    continue;

                using (var stream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(stream))
                    {
                        var buffer = packet.PayloadBuffer.ToArray();

                        binaryWriter.Write((byte)packet.Type);
                        binaryWriter.Write(buffer.Length);
                        binaryWriter.Write(buffer);

                        _clientSocket.SendAsync(
                                new ArraySegment<byte>(stream.ToArray()),
                                WebSocketMessageType.Binary,
                                true,
                                CancellationToken.None)
                                    .ConfigureAwait(false)
                                    .GetAwaiter()
                                    .GetResult();

                    }
                }

                await Task.Delay(10);
            }
        }

        private void DataRecive(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketTypeEnum.Video:

                    VideoDecoder?.Decode(packet.PayloadBuffer);
                    break;

                case PacketTypeEnum.Audio:
                    var buffer = packet.PayloadBuffer;
                    OutputAudioDevice?.PlaySamples(buffer, 60);

                    break;
                default:
                    break;
            }

            //packet.PayloadStream.Dispose();
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {

                bitmap.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            VideoDevice.Stop();
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

        private void CameraOnButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDevice.Start();
        }

        private void CameraOffButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDevice.Stop();
        }
    }
}
