using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddSingleton<Services.Interfaces.IChatService, Services.Network.ChatService>();
            _ = services.AddSingleton<Services.Interfaces.IFileTransferService, Services.Network.FileTransferService>();
            _ = services.AddSingleton<Services.Interfaces.IMessageRepository, Services.Data.MessageRepository>();
            _ = services.AddTransient<MainWindow>();
        }
    }

}
