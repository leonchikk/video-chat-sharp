using System;
using System.Linq;

namespace VoiceChat.Desktop.ViewModels
{
    public class ConnectionViewModel : ViewModelBase, IEquatable<ConnectionViewModel>
    {
        public string AccountId { get; private set; }
        public string Nickname { get; private set; }
        public char? FirstLetter => Nickname == null ? null : char.ToUpper(Nickname.First());

        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                OnPropertyChanged(nameof(IsMuted));
            }
        }

        public ConnectionViewModel(string accountId, string nickname)
        {
            AccountId = accountId;
            Nickname = nickname;
        }

        public bool Equals(ConnectionViewModel other)
        {
            return AccountId == other.AccountId;
        }
    }
}
