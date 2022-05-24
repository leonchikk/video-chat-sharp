using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VoiceChat.Desktop.ViewModels;

namespace VoiceChat.Desktop.Windows
{
    public partial class SplashWindow : Window
    {
        private readonly SignInWindow _signInWindow;
        private readonly SplashViewModel _splashViewModel;

        public SplashWindow(SignInWindow signInWindow, SplashViewModel splashViewModel)
        {
            DataContext = _splashViewModel;

            InitializeComponent();

            _signInWindow = signInWindow;
            _splashViewModel = splashViewModel;

            _splashViewModel.OnInitHandshake += OnInitHandshake;

            //Crutch to be able move window with none style
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void OnInitHandshake()
        {
           Dispatcher.Invoke(() =>
           {
               _signInWindow.Show();
               Close();
           });
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            await _splashViewModel.Connect();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _splashViewModel.Dispose();
        }
    }
}
