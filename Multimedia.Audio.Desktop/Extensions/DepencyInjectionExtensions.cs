using Microsoft.Extensions.DependencyInjection;
using VideoChat.Core.Multimedia;

namespace Multimedia.Audio.Desktop.Extensions
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddAudioInputOutputDevices(this IServiceCollection services)
        {
            services.AddSingleton<IInputAudioDevice, InputAudioDevice>();
            services.AddSingleton<IOutputAudioDevice, OutputAudioDevice>();

            return services;
        }
    }
}
