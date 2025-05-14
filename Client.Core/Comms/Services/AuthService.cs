using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client.Core.Comms.Services
{
    public class AuthService : Interfaces.IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly Interfaces.ITokenStorageService _tokenStorage;

        public AuthService(HttpClient httpClient, Interfaces.ITokenStorageService tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        public async Task<bool> RequestVerificationCodeAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/request-code", new { Email = email });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> VerifyCodeAndLoginAsync(string email, string code)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/verify-code", new { Email = email, Code = code });
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadFromJsonAsync<Entities.Dtos.JwtTokenDto>();
            if (json?.Token == null)
                return false;

            await _tokenStorage.SaveTokenAsync(json.Token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.Token);

            return true;
        }

        public async Task<string?> GetStoredTokenAsync()
        {
            return await _tokenStorage.LoadTokenAsync();
        }

        public async Task LogoutAsync()
        {
            await _tokenStorage.DeleteTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

    }
}
