using Microsoft.Extensions.DependencyInjection;
using Multimedia.Audio.Desktop.Extensions;
using RNNoiseWrapper.Extensions;
using System.Windows;
using VoiceEngine.Network.Extensions;

namespace VoiceChat.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<Views.SplashScreen>();
            services.AddNetworkInfrastructure();
            services.AddAudioInputOutputDevices();
            services.AddOpusCodec();
            services.AddNoiseReducer();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<Views.SplashScreen>();
            mainWindow.Show();
        }
    }
}
