using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace VoiceChat.Desktop.Views
{
    public partial class SplashScreen : Window
    {
        private readonly DispatcherTimer _showingTimer;
        private readonly MainWindow _mainWindow;

        public SplashScreen(MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
            _showingTimer = new DispatcherTimer();
            _showingTimer.Interval = TimeSpan.FromSeconds(2);
            _showingTimer.Tick += _showingTimer_Tick; 
            _showingTimer.Start();
        }

        private void _showingTimer_Tick(object sender, EventArgs e)
        {
            _showingTimer.Stop();
            _mainWindow.Show();

            Close();
        }
    }
}
