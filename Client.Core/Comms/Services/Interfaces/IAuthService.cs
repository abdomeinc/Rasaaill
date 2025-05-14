namespace Client.Core.Comms.Services.Interfaces
{
    /// <summary>
    /// Provides authentication services including login, logout, token management, and user verification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Logs out the current user and clears authentication data.
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Gets a value indicating whether the user is currently authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the currently authenticated user, or null if not authenticated.
        /// </summary>
        Entities.Dtos.UserDto? CurrentUser { get; }

        /// <summary>
        /// Gets the current access token, or null if not available.
        /// </summary>
        string? AccessToken { get; }

        /// <summary>
        /// Attempts to automatically log in the user using stored credentials or tokens.
        /// </summary>
        /// <returns>True if auto-login succeeded; otherwise, false.</returns>
        Task<bool> TryAutoLoginAsync();

        /// <summary>
        /// Requests a verification code to be sent to the specified email address.
        /// </summary>
        /// <param name="email">The email address to send the verification code to.</param>
        /// <returns>True if the request was successful; otherwise, false.</returns>
        Task<bool> RequestVerificationCodeAsync(string email);

        /// <summary>
        /// Verifies the provided code and logs in the user if successful.
        /// </summary>
        /// <param name="email">The email address associated with the code.</param>
        /// <param name="code">The verification code to validate.</param>
        /// <returns>True if verification and login succeeded; otherwise, false.</returns>
        Task<bool> VerifyCodeAndLoginAsync(string email, string code);

        /// <summary>
        /// Validates the specified authentication token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Retrieves the stored authentication token, if available.
        /// </summary>
        /// <returns>The stored token, or null if not found.</returns>
        Task<string?> GetStoredTokenAsync();
    }
}
