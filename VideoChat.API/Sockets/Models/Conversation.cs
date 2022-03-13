using System.Collections.Generic;

namespace VideoChat.API.Sockets.Models
{
    public class Conversation
    {
        public string Id { get; set; }
        public UserConnection Creator { get; set; }
        public IEnumerable<UserConnection> Participants { get; set; }

        public override bool Equals(object obj)
        {
            return (obj as Conversation).Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
