namespace VoiceEngine.Network.Abstractions.Models
{
    public class UserInfoModel
    {
        public string AccountId { get; set; }
        public string Nickname { get; set; }

        public UserInfoModel(string accountId, string nickname)
        {
            AccountId = accountId;
            Nickname = nickname;
        }
    }
}
