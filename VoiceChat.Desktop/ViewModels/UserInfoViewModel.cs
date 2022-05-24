using System.Linq;

namespace VoiceChat.Desktop.ViewModels
{
    public class UserInfoViewModel : ViewModelBase
    {
        public string Nickname { get; set; }
        public string AccountId { get; set; }
        public char? FirstLetter => Nickname == null ? null : char.ToUpper(Nickname.First());
    }
}
