using System;
using System.Windows.Input;
using VoiceChat.Desktop.Commands;
using VoiceEngine.Network.Abstractions.Clients;

namespace VoiceChat.Desktop.ViewModels
{
    public class SignInViewModel: ViewModelBase
    {
        private readonly ISocketClient _socketClient;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(CanSignIn));
            }
        }

        public event Action OnComplete;
        public ICommand SignInCommand { get; }

        public SignInViewModel(ISocketClient socketClient)
        {
            _socketClient = socketClient;

            SignInCommand = new SignInCommand(_socketClient, this);
        }

        public bool CanSignIn => !string.IsNullOrWhiteSpace(UserName);

        public void Complete()
        {
            OnComplete?.Invoke();
        }
    }
}
