namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for sending and verifying email-based verification codes.
    /// </summary>
    public interface IVerificationService
    {
        /// <summary>
        /// Sends a verification code to the specified email address asynchronously.
        /// </summary>
        /// <param name="email">The email address to which the verification code will be sent.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the verification code sent.
        /// </returns>
        Task<string> SendVerificationCodeAsync(string email);

        /// <summary>
        /// Verifies the provided code for the specified email address asynchronously.
        /// </summary>
        /// <param name="email">The email address associated with the verification code.</param>
        /// <param name="code">The verification code to verify.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with:
        /// <list type="bullet">
        /// <item><description>Success: Indicates whether the verification was successful.</description></item>
        /// <item><description>Error: An error message if the verification failed; otherwise, null.</description></item>
        /// <item><description>User: The user entity if verification succeeded; otherwise, null.</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, string? Error, Entities.Models.User? User)> VerifyCodeAsync(string email, string code);
    }
}
