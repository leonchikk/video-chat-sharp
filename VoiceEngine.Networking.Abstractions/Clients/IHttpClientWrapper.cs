using System;
using System.Threading.Tasks;

namespace VoiceEngine.Network.Abstractions.Clients
{
    public interface IHttpClientWrapper: IDisposable
    {
        Task<string> GetAuthorizationToken();
    }
}
