using Microsoft.Extensions.DependencyInjection;
using VideoChat.Core.Networking;

namespace Networking.Extensions
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddNetworkInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            services.AddSingleton<IWebSocketClient, WebSocketClient>();

            return services;
        }
    }
}
