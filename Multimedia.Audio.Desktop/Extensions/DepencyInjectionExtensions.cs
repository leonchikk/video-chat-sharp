using Microsoft.Extensions.DependencyInjection;
using VoiceEngine.Abstractions.IO;

namespace VoiceEngine.IO.Desktop.DependencyInjection
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddInputOutputInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IInputAudioDevice, InputAudioDevice>();
            services.AddSingleton<IOutputAudioDevice, OutputAudioDevice>();
            services.AddSingleton<IAudioRecorder, AudioRecorder>();

            return services;
        }
    }
}
