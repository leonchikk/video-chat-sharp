
using Multimedia.Audio.Desktop;
using Multimedia.Video.Desktop;
using Multimedia.Video.Desktop.Codecs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VideoChat.Core.Enumerations;
using VideoChat.Core.Models;
using VideoChat.Core.Multimedia;
using VideoChat.Core.Packets;

namespace VideoChat.Desktop
{
    public partial class MainWindow : Window
    {
        private object syncObj = new object();
        private readonly ClientWebSocket _clientSocket;

        public IVideoDeviceManager VideoDeviceManager;
        public IAudioDeviceManager AudioDeviceManager;

        public event Action<Packet> OnDataReceive;

        public MainWindow()
        {
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
                _ = Receive();

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
                    byte[] tempStorage = new byte[2]; // 1016 is one chunck
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
                                PayloadStream = new MemoryStream(payload),
                                Type = packetTypeEnum
                            };

                            OnDataReceive?.Invoke(packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }

        public void ConfigWebCams()
        {
            VideoDeviceManager = new VideoDevice(new H264Codec());
            //TODO: Add logic if there no device use just audio
            VideoDeviceManager.SetupDevice(VideoDeviceManager.AvailableDevices[0]);
            VideoDeviceManager.SetupCodec(VideoDeviceManager.DeviceCapabilities[0].FrameSize,
                500, VideoDeviceManager.DeviceCapabilities[0].FrameRate);
            VideoDeviceManager.OnCaptureNewFrames += CaptureNewFrame;

            AudioDeviceManager = new AudioDevice();
            AudioDeviceManager.OnSampleRecorded += SampleRecorded;
            AudioDeviceManager.Setup(
                AudioDeviceManager.InputDeviceCapabilities.First(),
                AudioDeviceManager.OutputDeviceCapabilities.First()
            );
        }

        private void CaptureNewFrame(byte[] buffer)
        {
            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.Video);
                    binaryWriter.Write(buffer.Length);
                    binaryWriter.Write(buffer);

                    lock(syncObj)
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
        }

        private void SampleRecorded(AudioSampleRecordedEventArgs e)
        {
            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((byte)PacketTypeEnum.Audio);
                    binaryWriter.Write(e.Buffer.Length);
                    binaryWriter.Write(e.Buffer);

                    lock(syncObj)
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
        }

        //private void CaptureNewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        //{
        //    //if (!client.IsAlive)
        //    //    client.Connect();

        //    GC.Collect();
        //    var frame = eventArgs.Frame;


        //    ////Dispatcher.BeginInvoke(new Action(() =>
        //    ////{
        //    ////    VideoField.Source = ToBitmapImage(frame);
        //    ////}));d

        //    ////Dispatcher.CurrentDispatcher.Invoke(() =>
        //    ////{

        //    string message = "It's whatever VSProtocol message!";

        //    System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

        //    MemoryStream ms = new MemoryStream();
        //    img.Save(ms, ImageFormat.Jpeg);
        //    ms.Seek(0, SeekOrigin.Begin);
        //    BitmapImage bi = new BitmapImage();
        //    bi.BeginInit();
        //    bi.StreamSource = ms;
        //    bi.EndInit();
        //    bi.Freeze();

        //    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\test.jpg";
        //    img.Save(path, ImageFormat.Jpeg);

        //    var packet = PacketBuilder.MakePacket(message, bi);
        //    var comp = packet.Compress();

        //    Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            Label.Content = $"Sending video.";
        //        }));

        //    _clientSocket.SendAsync(
        //              new ArraySegment<byte>(packet),
        //              WebSocketMessageType.Binary,
        //              true,
        //              CancellationToken.None)
        //       .ConfigureAwait(false)
        //       .GetAwaiter()
        //       .GetResult();
        //}

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDeviceManager.Start(VideoDeviceManager.DeviceCapabilities[1]);
            AudioDeviceManager.Start();

            Label.Content = "Start video sending";
        }

        private Packet ParseByteArray(byte[] buffer)
        {
            if (buffer.Length == 0)
                return null;


            return new Packet
            {
                //Frame = buffer
            };
        }

        private void DataRecive(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketTypeEnum.Video:
                    foreach (var decodedImage in VideoDeviceManager.Decode(packet.PayloadStream.ToArray()))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            VideoField.Source = ToBitmapImage(decodedImage);
                        }));
                    }
                    break;
                case PacketTypeEnum.Audio:
                    var buffer = packet.PayloadStream.ToArray();
                    AudioDeviceManager.PlaySamples(buffer, 60);
                    break;
                default:
                    break;
            }

            //packet.PayloadStream.Dispose();
        }

        public static BitmapImage ByteArrayToImage(Byte[] imageData)
        {
            using (var ms = new System.IO.MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();

                return image;
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            var memory = new MemoryStream();

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

        private void Window_Closed(object sender, EventArgs e)
        {
            _clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            VideoDeviceManager.Stop();
        }
    }
}
