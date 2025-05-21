using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

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

        [ObservableProperty]
        private int _selectedNavigationIndex;

        [ObservableProperty]
        private int _selectedFooterNavigationIndex;

        [ObservableProperty]
        private ObservableCollection<Models.NavigationItem> _navigationItems = new()
        {
            new Models.NavigationItem()
            {
                Name = "Conversations",
                Icon = Helpers.IconsLoader.Icon.ChatBubbles,
                DataType = typeof(ConversationsViewModel)
            },
            new Models.NavigationItem()
            {
                Name = "Calls",
                Icon = Helpers.IconsLoader.Icon.Phone,
                DataType = typeof(CallsViewModel)
            },
            new Models.NavigationItem()
            {
                Name = "Moments",
                //Icon = "\xE72D",
                Icon = Helpers.IconsLoader.Icon.StatusCircle,
                DataType = typeof(MomentsViewModel)
            },
            new Models.NavigationItem()
            {
                Name = "Groups",
                //Icon = "\xE716",
                Icon = Helpers.IconsLoader.Icon.Group,
                DataType = typeof(GroupsViewModel)
            },
            new Models.NavigationItem()
            {
                Name = "Archived",
                Icon = Helpers.IconsLoader.Icon.Package,
                DataType = typeof(ArchivedViewModel)
            }
        };

        [ObservableProperty]
        private ObservableCollection<Models.NavigationItem> _footerNavigationItems = new()
        {
            new Models.NavigationItem()
            {
                Name = "Settings",
                Icon = Helpers.IconsLoader.Icon.Settings,
                DataType = typeof(SettingsViewModel)
            },
            new Models.NavigationItem()
            {
                Name = "Profile",
                Icon = Helpers.IconsLoader.Icon.GuestUser,
                DataType = typeof(ProfileViewModel)
            }
        };

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

        partial void OnSelectedNavigationIndexChanged(int value)
        {
            if (value == -1) return;
            SelectedFooterNavigationIndex = -1;

            if (value == 0)
                SwitchToScreen(Shared.ScreenType.Conversations);
            else if (value == 1)
                SwitchToScreen(Shared.ScreenType.Settings);
        }

        partial void OnSelectedFooterNavigationIndexChanged(int value)
        {
            if (value == -1) return;
            SelectedNavigationIndex = -1;

            if (value == 0)
                SwitchToScreen(Shared.ScreenType.Settings);
            else if (value == 1)
                SwitchToScreen(Shared.ScreenType.Profile);

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
