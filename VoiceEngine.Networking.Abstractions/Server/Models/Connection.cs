namespace VoiceEngine.Network.Abstractions.Server.Models
{
    public class Connection
    {
        public Connection(string accountId, ISocket socket)
        {
            AccountId = accountId;
            Socket = socket;
            NickName = null;
        }

        public string AccountId { get; private set; }
        public string NickName { get; set; }
        public ISocket Socket { get; private set; }

        public bool FinishedHandshake => !string.IsNullOrWhiteSpace(NickName);
    }
}
