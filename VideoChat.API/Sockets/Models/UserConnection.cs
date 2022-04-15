using System.Net.WebSockets;

namespace VoiceEngine.API.Sockets.Models
{
    public class UserConnection
    {
        public string AccountId { get; set; }
        public WebSocket Socket { get; set; }
        public string Nickname { get; set; }

        public override bool Equals(object obj)
        {
            return (obj as UserConnection).AccountId == this.AccountId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
