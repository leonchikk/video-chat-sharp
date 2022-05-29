using System.Linq;
using VoiceEngine.Network.Abstractions.Models;

namespace VoiceEngine.Network.Abstractions.Server.Extensions
{
    public static class CollectionExtensions
    {
        public static UserInfoModel[] GetAllExept(this IConnectionManager connectionManager, string accountId)
        {
            return connectionManager.Get()
                                    .Where(x => x.AccountId != accountId)
                                    .Where(x => x.FinishedHandshake)
                                    .Select(x => new UserInfoModel(x.AccountId, x.NickName))
                                    .ToArray();
        }
    }
}
