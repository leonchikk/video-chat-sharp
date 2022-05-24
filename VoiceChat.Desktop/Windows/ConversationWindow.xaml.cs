using System.Windows;
using VoiceChat.Desktop.ViewModels;

namespace VoiceChat.Desktop.Windows
{
    public partial class ConversationWindow : Window
    {
        private readonly ConversationViewModel _conversationViewModel;

        public ConversationWindow(ConversationViewModel conversationViewModel)
        {
            InitializeComponent();

            //Crutch to be able move window with none style
            MouseLeftButtonDown += (s, e) => DragMove();

            _conversationViewModel = conversationViewModel;

            DataContext = _conversationViewModel;
        }
    }
}
