﻿using Microsoft.Extensions.DependencyInjection;
using VoiceEngine.Abstractions.IO;

namespace VoiceEngine.IO.Desktop.Extensions
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
