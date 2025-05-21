using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Client.WPF
{
    public partial class App
    {

        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory) ?? @"C:\"); })
                .ConfigureServices((context, services) =>
                {


                    ConfigureServices(services);
                })
                .Build();


        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }

        private static async void ConfigureServices(IServiceCollection services)
        {
            // Register HttpClient with base address
            _ = services.AddHttpClient<Core.Comms.Services.Interfaces.IAuthService, Core.Comms.Services.AuthService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
            });

            // Core services
            _ = services.AddSingleton<Core.Comms.Services.Interfaces.ITokenStorageService, Core.Comms.Services.TokenStorageService>();
            _ = services.AddSingleton<Services.Interfaces.INavigateService, Services.NavigateService>();
            _ = services.AddSingleton<Services.Interfaces.IUserStore, Services.UserStore>();
            _ = services.AddSingleton<Services.Interfaces.IConversationGeneratorService, Services.ConversationGeneratorService>();


            // ViewModels
            _ = services.AddTransient<ViewModels.MainViewModel>();

            _ = services.AddTransient<ViewModels.ArchivedViewModel>();
            _ = services.AddTransient<ViewModels.CallsViewModel>();
            _ = services.AddTransient<ViewModels.ConversationsViewModel>();
            _ = services.AddTransient<ViewModels.ConversationViewModel>();
            _ = services.AddTransient<ViewModels.EmojiPickerViewModel>();
            _ = services.AddTransient<ViewModels.GroupsViewModel>();
            _ = services.AddTransient<ViewModels.LoginVerificationViewModel>();
            _ = services.AddTransient<ViewModels.LoginViewModel>();
            _ = services.AddTransient<ViewModels.MainViewModel>();
            _ = services.AddTransient<ViewModels.MomentsViewModel>();
            _ = services.AddTransient<ViewModels.ProfileViewModel>();
            _ = services.AddTransient<ViewModels.SettingsViewModel>();
            _ = services.AddTransient<ViewModels.SplashViewModel>();
            _ = services.AddTransient<ViewModels.VerificationCodeViewModel>();


            _ = services.AddTransient<MainWindow>();

            await Helpers.IconsLoader.ReadIconData();

            await Helpers.EmojiManager.LoadAllEmojiMetadataAsync();
        }



        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();

            // Start main window    
            MainWindow mainWindow = /*new()*/_host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _host.Services.GetRequiredService<ViewModels.MainViewModel>();

            mainWindow.Show();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
