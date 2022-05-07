namespace VoiceEngine.Network.Abstractions.Server.Models
{
    public class Connection
    {
        public Connection(string accountId, ISocket socket)
        {
            AccountId = accountId;
            Socket = socket;
        }

        public string AccountId { get; private set; }
        public ISocket Socket { get; private set; }
    }
}
