using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Client.WPF
{
    public partial class App : Application
    {

        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            _host.Start();


            // Start main window    
            var mainWindow = new MainWindow
            {
                DataContext = _host.Services.GetRequiredService<ViewModels.MainViewModel>()
            };

            mainWindow.Show();

            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register HttpClient with base address
            services.AddHttpClient<Core.Comms.Services.Interfaces.IAuthService, Core.Comms.Services.AuthService>(client =>
            {
                client.BaseAddress = new Uri("https://your-api-base-url/");
            });

            // Core services
            services.AddSingleton<Core.Comms.Services.Interfaces.ITokenStorageService, Core.Comms.Services.TokenStorageService>();

            // ViewModels
            services.AddTransient<ViewModels.MainViewModel>();

            services.AddTransient<ViewModels.SplashViewModel>();
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.VerificationCodeViewModel>();

            services.AddTransient<ViewModels.ConversationsViewModel>();
            services.AddTransient<ViewModels.ConversationViewModel>();

            services.AddTransient<ViewModels.SettingsViewModel>();
            services.AddTransient<ViewModels.ProfileViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<Views.LoginView>();

            // Register other services
            // services.AddHttpClient<IAuthService, AuthService>();
            // services.AddSingleton<IOtherService, OtherService>();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
                await _host.StopAsync();

            base.OnExit(e);
        }
    }

}
