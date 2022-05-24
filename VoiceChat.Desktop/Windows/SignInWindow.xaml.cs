using System.Windows;
using VoiceChat.Desktop.ViewModels;

namespace VoiceChat.Desktop.Windows
{
    public partial class SignInWindow : Window
    {
        private readonly ConversationWindow _conversationWindow;
        private readonly SignInViewModel _signInViewModel;

        public SignInWindow(ConversationWindow conversationWindow, SignInViewModel signInViewModel)
        {
            _conversationWindow = conversationWindow;
            _signInViewModel = signInViewModel;
            _signInViewModel.OnComplete += OnCompleteSignIn;

            DataContext = _signInViewModel;

            //Crutch to be able move window with none style
            MouseLeftButtonDown += (s, e) => DragMove();

            InitializeComponent();
        }

        private void OnCompleteSignIn()
        {
            _conversationWindow.Show();

            Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _signInViewModel.OnComplete -= OnCompleteSignIn;
        }
    }
}
