using System;
using System.Linq;

namespace VoiceChat.Desktop.ViewModels
{
    public class ConnectionViewModel : ViewModelBase, IEquatable<ConnectionViewModel>
    {
        public string AccountId { get; set; }
        public string Nickname { get; set; }
        public char? FirstLetter => Nickname == null ? null : char.ToUpper(Nickname.First());

        public bool Equals(ConnectionViewModel other)
        {
            return AccountId == other.AccountId;
        }
    }
}
