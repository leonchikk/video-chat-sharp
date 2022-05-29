using VoiceChat.Desktop.ViewModels;

namespace VoiceChat.Desktop.Commands
{
    public class SwitchMicroStateCommand : CommandBase
    {
        private readonly UserInfoViewModel _userInfoViewModel;

        public SwitchMicroStateCommand(UserInfoViewModel userInfoViewModel)
        {
            _userInfoViewModel = userInfoViewModel;
        }

        public override void Execute(object parameter)
        {
            _userInfoViewModel.IsMicroOn = !_userInfoViewModel.IsMicroOn;
        }
    }
}
