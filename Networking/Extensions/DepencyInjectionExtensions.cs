using Microsoft.Extensions.DependencyInjection;
using VoiceEngine.Network.Abstractions.Clients;

namespace VoiceEngine.Network.DependencyInjection
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddNetworkInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IRestClient, RestClient>();
            services.AddSingleton<ISocketClient, WebSocketClient>();

            return services;
        }
    }
}
