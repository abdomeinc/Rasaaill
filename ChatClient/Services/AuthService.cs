using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatClient.Services
{
    public class AuthService : Interfaces.IAuthService
    {
        private const string FirebaseApiKey = "YOUR_FIREBASE_WEB_API_KEY";
        private readonly HttpClient _httpClient = new();
        private Shared.Models.User? _currentUser;

        public Shared.Models.User? CurrentUser => _currentUser;

        public AuthService()
        {
        }

        public async Task<Shared.Models.User?> Authenticate(string email, string password)
        {
            if (!email.EndsWith("@yourcompany.com", StringComparison.OrdinalIgnoreCase))
                //throw new AuthException("Invalid company domain");
                throw new Exception("Invalid company domain");

            var content = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}",
                content);

            if (!response.IsSuccessStatusCode)
                //throw new AuthException("Authentication failed");
                throw new Exception("Authentication failed");

            var result = await JsonSerializer.DeserializeAsync<FirebaseAuthResponse>(
                await response.Content.ReadAsStreamAsync());

            if (result is null)
                return null;

            return new Shared.Models.User
            {
                Id = result.localId,
                Email = result.email,
                DisplayName = result.displayName
            };
        }

        public Task Logout()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }
    }

    public class FirebaseAuthResponse
    {
        public string localId { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string idToken { get; set; } = string.Empty;
    }
}
