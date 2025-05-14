using Client.Core.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
        private static readonly string MetaFilePath = Path.Combine(TokenDirectory, "jwt.meta.json");

        /// <summary>
        /// Encrypts and saves the access and refresh tokens to disk.
        /// </summary>
        /// <param name="accessToken">The access token to store.</param>
        /// <param name="refreshToken">The refresh token to store.</param>
        public async Task SaveTokenAsync(string accessToken, string refreshToken)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(AccessTokenFile)!);

            var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(accessToken), null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(AccessTokenFile, encrypted);

            var meta = new
            {
                RefreshToken = refreshToken,
                SavedAt = DateTime.UtcNow,
                ExpiresAt = JwtDecoder.GetExpiration(accessToken)?.ToUniversalTime()
            };

            var metaJson = JsonSerializer.Serialize(meta);
            var encryptedMeta = ProtectedData.Protect(Encoding.UTF8.GetBytes(metaJson), null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(MetaFilePath, encryptedMeta);
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
            var accessToken = await LoadTokenAsync();
            if (accessToken == null) return ("", "");

            try
            {
                if (!File.Exists(MetaFilePath)) return (accessToken, "");

                var encryptedMeta = await File.ReadAllBytesAsync(MetaFilePath);
                var decryptedMeta = ProtectedData.Unprotect(encryptedMeta, null, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(decryptedMeta);
                var meta = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                var refreshToken = meta?["RefreshToken"]?.ToString() ?? "";
                return (accessToken, refreshToken);
            }
            catch
            {
                return (accessToken, "");
            }
        }

        /// <summary>
        /// Loads and decrypts the access token from disk.
        /// </summary>
        /// <returns>
        /// The access token as a string, or an empty string if not found or decryption fails.
        /// </returns>
        public async Task<string?> LoadTokenAsync()
        {
            try
            {
                if (!File.Exists(AccessTokenFile)) return null;

                var encrypted = await File.ReadAllBytesAsync(AccessTokenFile);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                await DeleteTokenAsync(); // Wipe corrupted files
                return null;
            }
        }

        /// <summary>
        /// Deletes both the access and refresh token files from disk.
        /// </summary>
        /// <returns>A completed task.</returns>
        public async Task DeleteTokenAsync()
        {
            try { if (File.Exists(AccessTokenFile)) File.Delete(AccessTokenFile); } catch { }
            try { if (File.Exists(MetaFilePath)) File.Delete(MetaFilePath); } catch { }
            await Task.CompletedTask;
        }
    }
}
