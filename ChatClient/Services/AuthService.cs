namespace ChatClient.Services
{
    public class AuthService : Interfaces.IAuthService
    {
        private readonly FirebaseAuthProvider _authProvider;
        private Shared.Models.User? _currentUser;

        public AuthService()
        {
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig("your-firebase-api-key"));
        }

        public Shared.Models.User? CurrentUser => _currentUser;

        public async Task<Shared.Models.User> Authenticate(string email, string password)
        {
            if (!email.EndsWith("@yourcompany.com", StringComparison.OrdinalIgnoreCase))
                //throw new AuthException("Invalid company domain");
                throw new Exception("Invalid company domain");

            var result = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
            _currentUser = new Shared.Models.User() { /*(result.User.Email, result.User.DisplayName)*/ };
            return _currentUser;
        }

        public Task Logout()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }
    }
}
