using System;
using System.ComponentModel;
using System.Threading.Tasks;
using VoiceChat.Desktop.Stores;
using VoiceChat.Desktop.ViewModels;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.Packets;

namespace VoiceChat.Desktop.Commands
{
    public class SignInCommand : AsyncCommandBase
    {
        private readonly ISocketClient _socketClient;
        private readonly SignInViewModel _signInViewModel;

        public SignInCommand(ISocketClient socketClient, SignInViewModel signInViewModel)
        {
            _socketClient = socketClient;
            _signInViewModel = signInViewModel;

            _signInViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return _signInViewModel.CanSignIn && base.CanExecute(parameter);
        }

        public async override Task ExecuteAsync(object parameter)
        {
            UserStore.Nickname = _signInViewModel.UserName;

            await _socketClient.SendPacket(new FinishHandshakePacket(UserStore.AccountId, UserStore.Nickname));

            _signInViewModel.Complete();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_signInViewModel.CanSignIn))
            {
                OnCanExecutedChanged();
            }
        }
    }
}
