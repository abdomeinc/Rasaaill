using System.Security.Cryptography;
using System.Text;

namespace Client.Core.Comms.Services
{
    /// <summary>
    /// Provides secure storage and retrieval of access and refresh tokens using Windows Data Protection API.
    /// Tokens are encrypted and saved to the user's application data folder.
    /// </summary>
    public class TokenStorageService : Interfaces.ITokenStorageService
    {
        private static readonly string TokenDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rasaaill");

        private static readonly string AccessTokenFile = Path.Combine(TokenDirectory, "access.token");
        private static readonly string RefreshTokenFile = Path.Combine(TokenDirectory, "refresh.token");

        /// <summary>
        /// Encrypts and saves the access and refresh tokens to disk.
        /// </summary>
        /// <param name="accessToken">The access token to store.</param>
        /// <param name="refreshToken">The refresh token to store.</param>
        public async Task SaveTokenAsync(string accessToken, string refreshToken)
        {
            Directory.CreateDirectory(TokenDirectory);

            var encryptedAccess = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(accessToken),
                null,
                DataProtectionScope.CurrentUser
            );

            var encryptedRefresh = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(refreshToken),
                null,
                DataProtectionScope.CurrentUser
            );

            await File.WriteAllBytesAsync(AccessTokenFile, encryptedAccess);
            await File.WriteAllBytesAsync(RefreshTokenFile, encryptedRefresh);
        }

        /// <summary>
        /// Loads and decrypts both the access and refresh tokens from disk.
        /// </summary>
        /// <returns>
        /// A tuple containing the access token and refresh token as strings.
        /// If a token is not found or cannot be decrypted, its value will be an empty string.
        /// </returns>
        public async Task<(string AccessToken, string RefreshToken)> LoadTokenAndRefreshAsync()
        {
            string accessToken = string.Empty;
            string refreshToken = string.Empty;

            if (File.Exists(AccessTokenFile))
            {
                try
                {
                    var encryptedAccess = await File.ReadAllBytesAsync(AccessTokenFile);
                    var decryptedAccess = ProtectedData.Unprotect(encryptedAccess, null, DataProtectionScope.CurrentUser);
                    accessToken = Encoding.UTF8.GetString(decryptedAccess);
                }
                catch { }
            }

            if (File.Exists(RefreshTokenFile))
            {
                try
                {
                    var encryptedRefresh = await File.ReadAllBytesAsync(RefreshTokenFile);
                    var decryptedRefresh = ProtectedData.Unprotect(encryptedRefresh, null, DataProtectionScope.CurrentUser);
                    refreshToken = Encoding.UTF8.GetString(decryptedRefresh);
                }
                catch { }
            }

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Loads and decrypts the access token from disk.
        /// </summary>
        /// <returns>
        /// The access token as a string, or an empty string if not found or decryption fails.
        /// </returns>
        public async Task<string> LoadTokenAsync()
        {
            if (!File.Exists(AccessTokenFile))
                return string.Empty;

            try
            {
                var encrypted = await File.ReadAllBytesAsync(AccessTokenFile);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes both the access and refresh token files from disk.
        /// </summary>
        /// <returns>A completed task.</returns>
        public Task DeleteTokenAsync()
        {
            if (File.Exists(AccessTokenFile))
                File.Delete(AccessTokenFile);

            if (File.Exists(RefreshTokenFile))
                File.Delete(RefreshTokenFile);

            return Task.CompletedTask;
        }
    }
}
