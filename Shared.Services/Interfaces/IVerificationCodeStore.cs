namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Defines methods for storing, retrieving, and removing verification codes associated with email addresses.
    /// </summary>
    public interface IVerificationCodeStore
    {
        /// <summary>
        /// Stores a verification code for the specified email address with an expiration time.
        /// </summary>
        /// <param name="email">The email address to associate with the verification code.</param>
        /// <param name="code">The verification code to store.</param>
        /// <param name="expiresIn">The duration after which the code expires.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StoreCodeAsync(string email, string code, TimeSpan expiresIn);

        /// <summary>
        /// Retrieves the verification code associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address whose verification code is to be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the verification code if found; otherwise, <c>null</c>.
        /// </returns>
        Task<string?> GetCodeAsync(string email);

        /// <summary>
        /// Removes the verification code associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address whose verification code is to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveCodeAsync(string email);
    }
}
