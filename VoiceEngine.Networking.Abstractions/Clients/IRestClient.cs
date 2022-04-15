using System;
using System.Threading.Tasks;

namespace VoiceEngine.Network.Abstractions.Clients
{
    public interface IRestClient: IDisposable
    {
        Task<string> GetAuthorizationToken();
    }
}
