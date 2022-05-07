using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using VoiceEngine.IO.Desktop.DependencyInjection;
using VoiceEngine.Network.DependencyInjection;
using VoiceEngine.Filters.DependencyInjection;

namespace VoiceChat.Desktop
{
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
            services.AddClientInfrastructure();
            services.AddInputOutputInfrastructure();
            services.AddOpusCodec();
            services.AddNoiseReducer();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            //var splashWindow = serviceProvider.GetService<Views.SplashScreen>();
            //splashWindow.Show();

            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
