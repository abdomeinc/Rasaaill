using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Client.WPF.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Services.Interfaces.INavigateService _navigateService;
        private readonly Services.Interfaces.IUserStore _userStore;

        [ObservableProperty]
        private bool isLoggedIn = false; // Property to track if the user is logged in


        [ObservableProperty]
        private string _applicationTitle = "Rasaaill | Instant Messages";

        //[ObservableProperty]
        //private ObservableCollection<object> _menuItems = new()
        //{
        //    new NavigationViewItem()
        //    {
        //        Content = "Conversations",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.Chat28 },
        //        TargetPageType = typeof(ConversationsViewModel)
        //    },
        //    new NavigationViewItem()
        //    {
        //        Content = "Calls",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.Call20 },
        //        TargetPageType = typeof(ProfileViewModel)
        //    },
        //    new NavigationViewItem()
        //    {
        //        Content = "Status",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.ResizeImage20 },
        //        TargetPageType = typeof(ProfileViewModel)
        //    },
        //    new NavigationViewItem()
        //    {
        //        Content = "Groups",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.PeopleCommunity20 },
        //        TargetPageType = typeof(ProfileViewModel)
        //    },
        //    new NavigationViewItem()
        //    {
        //        Content = "Archived",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.Archive32 },
        //        TargetPageType = typeof(ProfileViewModel)
        //    }
        //};

        //[ObservableProperty]
        //private ObservableCollection<object> _footerMenuItems = new()
        //{
        //    new NavigationViewItem()
        //    {
        //        Content = "Settings",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
        //        TargetPageType = typeof(SettingsViewModel)
        //    },
        //    new NavigationViewItem()
        //    {
        //        Content = "Profile",
        //        Icon = new SymbolIcon { Symbol = SymbolRegular.Person32 },
        //        TargetPageType = typeof(SettingsViewModel)
        //    }
        //};

        //[ObservableProperty]
        //private ObservableCollection<MenuItem> _trayMenuItems = new()
        //{
        //    new MenuItem { Header = "Home", Tag = "tray_home" }
        //};

        // Enum to define views

        [ObservableProperty]
        private Shared.ScreenType currentView = Shared.ScreenType.Splash;

        [ObservableProperty]
        private ObservableObject currentViewModel;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            currentViewModel = default!;
            _navigateService = _serviceProvider.GetRequiredService<Services.Interfaces.INavigateService>();
            _userStore = _serviceProvider.GetRequiredService<Services.Interfaces.IUserStore>();

            // Subscribe to screen changes
            _navigateService.OnScreenChanged += SwitchToScreen;
            // Subscribe to user sign-in changes
            _userStore.OnUserSignChanged += (user, isLoggedIn) =>
            {
                IsLoggedIn = isLoggedIn;
                if (isLoggedIn)
                {
                    SwitchToScreen(Shared.ScreenType.Conversations);
                }
                else
                {
                    SwitchToScreen(Shared.ScreenType.Splash);
                }
            };
            // Initialize the view model
            _ = SwitchToSplashIfNeededAsync();
        }

        private async Task SwitchToSplashIfNeededAsync()
        {
            Core.Comms.Services.Interfaces.IAuthService authService = _serviceProvider.GetRequiredService<Core.Comms.Services.Interfaces.IAuthService>();
            bool isLoggedIn = await authService.TryAutoLoginAsync();

            if (isLoggedIn && authService.IsAuthenticated)
            {
                _userStore.SetUser(authService.CurrentUser);
                return;
            }

            SwitchToScreen(Shared.ScreenType.Splash);
        }

        public void SwitchToScreen(Shared.ScreenType screen)
        {
            switch (screen)
            {
                case Shared.ScreenType.Splash:
                    CurrentViewModel = _serviceProvider.GetRequiredService<SplashViewModel>();
                    break;
                case Shared.ScreenType.Conversations:
                    CurrentViewModel = _serviceProvider.GetRequiredService<ConversationsViewModel>();
                    break;
                case Shared.ScreenType.Verification:
                    CurrentViewModel = _serviceProvider.GetRequiredService<LoginVerificationViewModel>();
                    break;
                case Shared.ScreenType.Login:
                    CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                    break;
                case Shared.ScreenType.Profile:
                    CurrentViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    break;
                case Shared.ScreenType.Settings:
                    CurrentViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                    break;
                default:
                    break;
            }
        }
    }
}
