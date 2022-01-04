using System.Windows;
using System.Linq;
using System.Net.WebSockets;
using System.Net.Http;
using System;
using System.Threading;

namespace VideoChat.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly WebSocket _socketClient;

        public MainWindow()
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync("https://localhost:44398/api/auth", null)
                           .ConfigureAwait(false)
                           .GetAwaiter()
                           .GetResult();

                var jwtToken = response.Content.ReadAsStringAsync()
                           .ConfigureAwait(false)
                           .GetAwaiter()
                           .GetResult();

                ClientWebSocket clientSocket = new ClientWebSocket();
                clientSocket.Options.SetRequestHeader("Authorization", jwtToken);
                clientSocket.ConnectAsync(new Uri("wss://localhost:44398"), CancellationToken.None);

                //clientSocket.SendAsync()
                //    "wss://localhost:44398"
                //_socketClient.SetCookie(new Cookie("Authorization", jwtToken));
            }

            InitializeComponent();
        }
    }
}
