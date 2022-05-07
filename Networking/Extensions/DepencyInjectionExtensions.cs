using Microsoft.Extensions.DependencyInjection;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.Server;
using VoiceEngine.Network.Abstractions.Services;
using VoiceEngine.Network.Server;
using VoiceEngine.Network.Services;

namespace VoiceEngine.Network.DependencyInjection
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddClientInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IRestClient, RestClient>();
            services.AddSingleton<ISocketClient, WebSocketClient>();
            services.AddSingleton<ITokenService, TokenService>();

            return services;
        }

        public static IServiceCollection AddServerInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IBroadcaster, Broadcaster>();

            return services;
        }
    }
}
