using Microsoft.Extensions.DependencyInjection;
using OpusWrapper;
using VideoChat.Core.Codec;
using VoiceEngine.Abstractions.Encoding;
using VoiceEngine.Abstractions.Filters;

namespace RNNoiseWrapper.Extensions
{
    public static class DepencyInjectionExtensions
    {
        public static IServiceCollection AddNoiseReducer(this IServiceCollection services)
        {
            services.AddSingleton<INoiseReducer, NoiseReducer>(factory =>
            {
                return new NoiseReducer(new NoiseReducerConfig()
                {
                    Attenuation = 40,
                    Model = RnNoiseModel.Speech,
                    NumChannels = (int)NumChannels.Mono,
                    SampleRate = (int)SampleRate._48000
                });
            });

            return services;
        }

        public static IServiceCollection AddOpusCodec(this IServiceCollection services)
        {
            services.AddSingleton<IAudioEncoder, Encoder>(factory =>
            {
                return new Encoder(new EncoderConfig()
                {
                    Application = Application.Voip,
                    Channels = (ushort)NumChannels.Mono,
                    SampleRate = (uint)SampleRate._48000
                });
            });

            services.AddSingleton<IAudioDecoder, Decoder>(factory =>
            {
                return new Decoder(new DecoderConfig()
                {
                    Channels = (ushort)NumChannels.Mono,
                    SampleRate = (uint)SampleRate._48000
                });
            });

            return services;
        }
    }
}
