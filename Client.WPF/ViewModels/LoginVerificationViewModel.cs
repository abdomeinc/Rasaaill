using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Windows.Threading;

namespace Client.WPF.ViewModels
{
    public partial class LoginVerificationViewModel : ViewModelBase
    {
        private readonly ILogger<LoginVerificationViewModel> _logger;
        private readonly Core.Comms.Services.Interfaces.IAuthService _authService; // Corrected namespace
        private readonly Services.Interfaces.INavigateService _navigateService;
        private readonly Services.Interfaces.IUserStore _userStore;

        // Properties for each digit of the verification code
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit2;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit3;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit4;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit5;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(VerifyCommand))] // Notify the command when this property changes
        private string? digit6;


        // Computed property to get the full verification code string
        public string VerificationCode => $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}{Digit6}";


        [ObservableProperty]
        private bool isVerifying; // To indicate if the verification process is ongoing (for wait animation)


        [ObservableProperty]
        private string emailAddress;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResendCodeCommand))]
        private bool canResend = false;

        [ObservableProperty]
        private string? remainingTimeDisplay;

        private DispatcherTimer _resendCooldownTimer = default!;
        private TimeSpan _cooldownDuration = TimeSpan.FromMinutes(2);
        private TimeSpan _remainingTime;


        public LoginVerificationViewModel(ILogger<LoginVerificationViewModel> logger,
                                        Core.Comms.Services.Interfaces.IAuthService authService,
                                        Services.Interfaces.INavigateService navigateService,
                                        Services.Interfaces.IUserStore userStore)
        {
            _logger = logger;
            _authService = authService;
            _navigateService = navigateService;
            _userStore = userStore;

            EmailAddress = _userStore.StoredUsername ?? "";

            // Initialize the remaining time display
            StartResendCooldown(); // start countdown immediately after sending code
        }

        private void StartResendCooldown()
        {
            CanResend = false;
            _remainingTime = _cooldownDuration;
            UpdateRemainingTimeDisplay();

            _resendCooldownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _resendCooldownTimer.Tick += (s, e) =>
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateRemainingTimeDisplay();

                if (_remainingTime <= TimeSpan.Zero)
                {
                    _resendCooldownTimer.Stop();
                    CanResend = true;
                    RemainingTimeDisplay = null;
                }
            };

            _resendCooldownTimer.Start();
        }

        private void UpdateRemainingTimeDisplay()
        {
            RemainingTimeDisplay = $"Request another code available in {_remainingTime:mm\\:ss}";
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            _navigateService.NavigateTo(Shared.ScreenType.Login);
        }
        [RelayCommand]
        private void NavigateToConversations()
        {
            _navigateService.NavigateTo(Shared.ScreenType.Conversations);
        }

        // Command to handle the verification button click
        [RelayCommand(CanExecute = nameof(CanVerify))]
        private async Task Verify()
        {
            // This check is technically redundant due to CanExecute but good practice
            if (!CanVerify())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_userStore.StoredUsername))
            {
                return;
            }

            IsVerifying = true; // Start busy indicator
            ErrorMessage = string.Empty; // Clear previous errors

            try
            {
                // Call your authentication service to verify the code
                // You'll need to pass the username along with the code
                bool isVerified = await _authService.VerifyCodeAndLoginAsync(_userStore.StoredUsername, VerificationCode);

                if (isVerified && _authService.IsAuthenticated)
                {
                    _logger.LogInformation("Verification successful.");
                    _userStore.SetUser(_authService.CurrentUser);
                    // Navigate to the next screen (e.g., Dashboard or Home)
                    _navigateService.NavigateTo(Shared.ScreenType.Conversations); // Adjust target screen type
                }
                else
                {
                    ErrorMessage = "Verification failed: Verification code expired or Invalid code. Please try again.";
                    _logger.LogWarning("Verification failed: Verification code expired or Invalid code.");
                    // Optionally clear the fields after a failed attempt
                    ClearDigits();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during verification: {ex.Message}";
                _logger.LogError(ex, "Error during verification.");
                // Optionally clear the fields on error
                ClearDigits();
            }
            finally
            {
                IsVerifying = false; // Stop busy indicator
                                     // Re-evaluate CanExecute state after busy state changes
                VerifyCommand.NotifyCanExecuteChanged();
                // Also notify for each digit property change to clear potential validation errors
                OnPropertyChanged(nameof(Digit1));
                OnPropertyChanged(nameof(Digit2));
                OnPropertyChanged(nameof(Digit3));
                OnPropertyChanged(nameof(Digit4));
                OnPropertyChanged(nameof(Digit5));
                OnPropertyChanged(nameof(Digit6));
            }
        }

        // CanExecute method for the VerifyCommand
        private bool CanVerify()
        {
            // The command can execute only if all 6 digit fields have a non-empty string
            // AND we are not currently busy with a verification request.
            return !string.IsNullOrWhiteSpace(Digit1) && Digit1.Length == 1 && char.IsDigit(Digit1[0]) &&
                   !string.IsNullOrWhiteSpace(Digit2) && Digit2.Length == 1 && char.IsDigit(Digit2[0]) &&
                   !string.IsNullOrWhiteSpace(Digit3) && Digit3.Length == 1 && char.IsDigit(Digit3[0]) &&
                   !string.IsNullOrWhiteSpace(Digit4) && Digit4.Length == 1 && char.IsDigit(Digit4[0]) &&
                   !string.IsNullOrWhiteSpace(Digit5) && Digit5.Length == 1 && char.IsDigit(Digit5[0]) &&
                   !string.IsNullOrWhiteSpace(Digit6) && Digit6.Length == 1 && char.IsDigit(Digit6[0]) &&
                   !IsVerifying;
        }

        // Helper method to clear the digit fields
        private void ClearDigits()
        {
            Digit1 = null;
            Digit2 = null;
            Digit3 = null;
            Digit4 = null;
            Digit5 = null;
            Digit6 = null;
            // Note: You might need to manage focus in the View's code-behind or with a behavior
            // to return focus to the first input field after clearing.
        }

        // You might also want a Resend Code command
        [RelayCommand(CanExecute = nameof(CanResend))]
        private async Task ResendCode()
        {
            try
            {
                IsVerifying = true;
                ErrorMessage = string.Empty; // Clear previous errors
                bool isCodeSent = await _authService.RequestVerificationCodeAsync(_userStore.StoredUsername!);
                if (isCodeSent)
                {
                    IsVerifying = false;
                }
                else
                {
                    IsVerifying = false;
                    ResendCodeCommand.NotifyCanExecuteChanged();
                    VerifyCommand.NotifyCanExecuteChanged();

                    ErrorMessage = "Faield to request another verification code.";
                }

                _logger.LogInformation("Verification code resent.");
                StartResendCooldown();
            }
            catch (Exception ex)
            {
                IsVerifying = false;
                _logger.LogError(ex, "Failed to resend verification code.");
                ErrorMessage = "Unable to resend verification code at this time.";
            }
        }
    }
}
