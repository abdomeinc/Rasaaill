using CommunityToolkit.Mvvm.Input;

namespace Mica.ViewModels
{
    public partial class SplashViewModel : ViewModelBase
    {
        private readonly Services.Interfaces.INavigateService _navigateService;
        public SplashViewModel(Services.Interfaces.INavigateService navigateService)
        {
            _navigateService = navigateService;
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            _navigateService.NavigateTo(Shared.ScreenType.Login);
        }
    }
}
