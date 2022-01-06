using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Image = System.Drawing.Image;

namespace VideoChat.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly ClientWebSocket _clientSocket;

        public FilterInfoCollection CaptureDevices;
        public VideoCaptureDevice CaptureDevice;

        public event Action<Packet> OnMessageReceive;

        public MainWindow()
        {
            using (var httpClient = new HttpClient())
            {
                //var response = httpClient.PostAsync("http://192.168.0.107:5000/api/auth", null)
                //           .ConfigureAwait(false)
                //           .GetAwaiter()
                //           .GetResult();

                //var jwtToken = response.Content.ReadAsStringAsync()
                //           .ConfigureAwait(false)
                //           .GetAwaiter()
                //           .GetResult();

                //_clientSocket = new ClientWebSocket();
                //_clientSocket.Options.SetRequestHeader("Authorization", jwtToken);
                //_clientSocket.ConnectAsync(new Uri("ws://192.168.0.107:5000"), CancellationToken.None);


                //var encoder = new ("openh264-2.1.1-win32.dll");

                OnMessageReceive += (packet) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        VideoField.Source = ByteArrayToImage(packet.Frame);
                    }));
                };

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

            //Wait until client made connection (but infact you should refactor this shit)

            await Task.Delay(4000);

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

                        var packet = ParseByteArray(buffer);

                        OnMessageReceive.Invoke(packet);
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
            CaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (CaptureDevices.Count > 0)
            {
                CaptureDevice = new VideoCaptureDevice(CaptureDevices[0].MonikerString);
                //_captureDevice.VideoResolution = VideoCapabilities
                CaptureDevice.NewFrame += CaptureNewFrame;
            }
        }

        private void CaptureNewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            //if (!client.IsAlive)
            //    client.Connect();

            GC.Collect();
            var frame = eventArgs.Frame;
           

            ////Dispatcher.BeginInvoke(new Action(() =>
            ////{
            ////    VideoField.Source = ToBitmapImage(frame);
            ////}));d

            ////Dispatcher.CurrentDispatcher.Invoke(() =>
            ////{

            string message = "It's whatever VSProtocol message!";

                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\test.jpg";
            img.Save(path, ImageFormat.Jpeg);

            var packet = PacketBuilder.MakePacket(message, bi);
            var comp = packet.Compress();

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    Label.Content = $"Sending video.";
                }));

             _clientSocket.SendAsync(
                       new ArraySegment<byte>(packet),
                       WebSocketMessageType.Binary,
                       true,
                       CancellationToken.None)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            CaptureDevice.Start();

            Label.Content = "Start video sending";
        }

        private Packet ParseByteArray(byte[] buffer)
        {
            if (buffer.Length == 0)
                return null;

          
            return new Packet
            {
                Frame = buffer
            };
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
            CaptureDevice.Stop();
        }
    }
}
