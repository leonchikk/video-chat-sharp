using Microsoft.Extensions.DependencyInjection;
using VoiceEngine.Network.Abstractions.Clients;

namespace VoiceEngine.Network.Extensions
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
