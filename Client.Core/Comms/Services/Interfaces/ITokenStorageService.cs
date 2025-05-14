namespace Client.Core.Comms.Services.Interfaces
{
    /// <summary>
    /// Defines methods for securely storing, retrieving, and deleting authentication tokens.
    /// </summary>
    public interface ITokenStorageService
    {
        /// <summary>
        /// Asynchronously saves the access and refresh tokens.
        /// </summary>
        /// <param name="accessToken">The access token to store.</param>
        /// <param name="refreshToken">The refresh token to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveTokenAsync(string accessToken, string refreshToken);

        /// <summary>
        /// Asynchronously loads the access and refresh tokens, refreshing them if necessary.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the access token and refresh token.
        /// </returns>
        Task<(string AccessToken, string RefreshToken)> LoadTokenAndRefreshAsync();

        /// <summary>
        /// Asynchronously loads the access token.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the access token.
        /// </returns>
        Task<string> LoadTokenAsync();

        /// <summary>
        /// Asynchronously deletes the stored tokens.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteTokenAsync();
    }
}
