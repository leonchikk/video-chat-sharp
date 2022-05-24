using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using VoiceEngine.IO.Desktop.DependencyInjection;
using VoiceEngine.Network.DependencyInjection;
using VoiceEngine.Filters.DependencyInjection;
using VoiceChat.Desktop.Windows;
using VoiceChat.Desktop.ViewModels;

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
            services.AddSingleton<ConversationWindow>();
            services.AddSingleton<SplashWindow>();
            services.AddSingleton<SignInWindow>();
            services.AddClientInfrastructure();
            services.AddInputOutputInfrastructure();
            services.AddOpusCodec();
            services.AddNoiseReducer();

            services.AddSingleton<ConversationViewModel>();
            services.AddSingleton<SplashViewModel>();
            services.AddSingleton<SignInViewModel>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var splashWindow = serviceProvider.GetService<SplashWindow>();

            splashWindow.Show();
        }
    }
}
