using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Client.WPF.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly ILogger<LoginViewModel> _logger;
        private readonly Core.Comms.Services.Interfaces.IAuthService _authService;
        private readonly Services.Interfaces.INavigateService _navigateService;
        private readonly Services.Interfaces.IUserStore _userStore;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ProceedCommand))] // This tells the toolkit to re-evaluate CanProceed when Username changes
        private string? username;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool isBusy;

        public LoginViewModel(ILogger<LoginViewModel> logger, Core.Comms.Services.Interfaces.IAuthService authService, Services.Interfaces.INavigateService navigateService, Services.Interfaces.IUserStore userStore)
        {
            _logger = logger;
            _navigateService = navigateService;
            _authService = authService;
            _userStore = userStore;
        }

        [RelayCommand]
        private void NavigateToSplash()
        {
            _navigateService.NavigateTo(Shared.ScreenType.Splash);
        }

        // This method defines the condition for the Proceed command
        private bool CanProceed()
        {
            // Also ensure we can't proceed if we are already busy
            bool result = !string.IsNullOrWhiteSpace(Username) && !IsBusy;
            return result;
        }

        // Link the CanExecute method to the RelayCommand
        [RelayCommand(CanExecute = nameof(CanProceed))]

        private async Task Proceed()
        {
            // The check inside the method is technically redundant because CanExecute handles it,
            // but it doesn't hurt to keep it as a safeguard.
            if (!CanProceed())
            {
                return;
            }

            ErrorMessage = string.Empty;
            IsBusy = true; // Set busy state to true when the command starts
            // Call the service to request a verification code
            try
            {
                string emailAddress = $"{Username}@misrtech-eg.com";
                bool isCodeSent = await _authService.RequestVerificationCodeAsync(emailAddress);
                if (isCodeSent)
                {
                    _userStore.StoredUsername = emailAddress;
                    _navigateService.NavigateTo(Shared.ScreenType.Verification);
                }
                else
                {

                    IsBusy = false; // Always reset busy state when the command finishes (success or failure)
                                    // Re-evaluate CanExecute as IsBusy has changed
                    ProceedCommand.NotifyCanExecuteChanged();

                    ErrorMessage = "Email not found.";
                }
            }
            catch (Exception ex)
            {
                IsBusy = false; // Always reset busy state when the command finishes (success or failure)
                // Re-evaluate CanExecute as IsBusy has changed
                ProceedCommand.NotifyCanExecuteChanged();
                // Handle the exception (e.g., show a message to the user)
                ErrorMessage = ex.Message;
                // For example, you can use a message box or a logging service
                _logger.LogError($"Error: {ex.Message}");
            }
        }
    }
}
