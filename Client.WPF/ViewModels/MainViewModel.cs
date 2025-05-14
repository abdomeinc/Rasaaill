using Client.Core.Comms.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Client.WPF.ViewModels
{
    public enum CurrentUserInterfaceView
    {
        Conversations,
        Settings,
        Profile
    }

    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;


        // Enum to define views

        [ObservableProperty]
        private CurrentUserInterfaceView currentView = CurrentUserInterfaceView.Conversations;

        [ObservableProperty]
        private ObservableObject currentViewModel;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            currentViewModel = _serviceProvider.GetRequiredService<SplashViewModel>()!;

            SwitchToLoginIfNeededAsync();
        }

        private async void SwitchToLoginIfNeededAsync()
        {

            var tokenService = _serviceProvider.GetRequiredService<ITokenStorageService>();
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var httpClient = _serviceProvider.GetRequiredService<HttpClient>();

            var token = await tokenService.LoadTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var isValid = await authService.ValidateTokenAsync(token);
                if (isValid)
                {
                    SwitchToConversations();
                    return;
                }
            }
            SwitchToLogin();
        }

        public void SwitchToSplash()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
        }


        public void SwitchToLogin()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        }

        [RelayCommand]
        public void SwitchToConversations()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ConversationsViewModel>();
        }

        [RelayCommand]
        public void SwitchToSettings()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        }

        [RelayCommand]
        public void SwitchToProfile()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
        }
    }
}
