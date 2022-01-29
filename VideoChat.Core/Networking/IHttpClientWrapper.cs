using System;
using System.Threading.Tasks;

namespace VideoChat.Core.Networking
{
    public interface IHttpClientWrapper: IDisposable
    {
        Task<string> GetAuthorizationToken();
    }
}
