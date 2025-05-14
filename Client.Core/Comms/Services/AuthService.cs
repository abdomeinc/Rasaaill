using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client.Core.Comms.Services
{
    /// <summary>
    /// Provides authentication services including login, logout, token validation, and user state management.
    /// </summary>
    public class AuthService : Interfaces.IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Interfaces.ITokenStorageService _tokenStorage;

        /// <summary>
        /// Gets a value indicating whether the user is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Gets the currently authenticated user, or null if not authenticated.
        /// </summary>
        public Entities.Dtos.UserDto? CurrentUser { get; private set; }

        /// <summary>
        /// Gets the current access token, or null if not authenticated.
        /// </summary>
        public string? AccessToken { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="httpClient">The HTTP client for API requests.</param>
        /// <param name="tokenStorage">The token storage service.</param>
        public AuthService(ILogger<AuthService> logger, HttpClient httpClient, Interfaces.ITokenStorageService tokenStorage)
        {
            _logger = logger;
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        /// <summary>
        /// Requests a verification code to be sent to the specified email address.
        /// </summary>
        /// <param name="email">The email address to send the verification code to.</param>
        /// <returns>True if the request was successful; otherwise, false.</returns>
        public async Task<bool> RequestVerificationCodeAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/request-verification-code", new Entities.Dtos.EmailDto() { Email = email });
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Verifies the provided code and logs in the user if successful.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="code">The verification code.</param>
        /// <returns>True if login was successful; otherwise, false.</returns>
        public async Task<bool> VerifyCodeAndLoginAsync(string email, string code)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/verify-verification-code", new Entities.Dtos.VerifyCodeDto() { Email = email, Code = code });
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadFromJsonAsync<Entities.Dtos.VerifyVerificationCodeResultDto>();

            if (json?.Token == null || json.RefreshToken == null)
                return false;

            await _tokenStorage.SaveTokenAsync(json.Token, json.RefreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.Token);

            CurrentUser = Helpers.JwtDecoder.ParseToken(json.Token);

            // Set access token and mark user as authenticated
            AccessToken = json.Token;
            IsAuthenticated = true;
            return true;
        }

        /// <summary>
        /// Retrieves the stored access token, if available.
        /// </summary>
        /// <returns>The stored access token, or null if not found.</returns>
        public async Task<string?> GetStoredTokenAsync()
        {
            return await _tokenStorage.LoadTokenAsync();
        }

        /// <summary>
        /// Logs out the current user and clears authentication state and tokens.
        /// </summary>
        public async Task LogoutAsync()
        {
            CurrentUser = null;
            AccessToken = null;
            IsAuthenticated = false;
            await _tokenStorage.DeleteTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Attempts to automatically log in the user using a stored token.
        /// </summary>
        /// <returns>True if auto-login was successful; otherwise, false.</returns>
        public async Task<bool> TryAutoLoginAsync()
        {
            var token = await GetStoredTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                IsAuthenticated = false;
                CurrentUser = null;
                return IsAuthenticated;
            }

            var isValid = await ValidateTokenAsync(token);
            if (!isValid)
            {
                await _tokenStorage.DeleteTokenAsync();
                IsAuthenticated = false;
                CurrentUser = null;
                return IsAuthenticated;
            }

            CurrentUser = Helpers.JwtDecoder.ParseToken(token);

            // Set access token and mark user as authenticated
            AccessToken = token;

            // Optionally decode token and set CurrentUser here
            IsAuthenticated = true;
            return IsAuthenticated;

        }

        /// <summary>
        /// Validates the specified access token with the authentication API.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/validate");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to validate token.");
                return false;
            }
        }
    }
}
