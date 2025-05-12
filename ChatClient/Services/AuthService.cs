using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;

namespace ChatClient.Services
{
    public class AuthService : Interfaces.IAuthService
    {
        //private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseAuthClient client;
        private Shared.Models.User? _currentUser;

        // Configure...
        FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = "<API KEY>",
            AuthDomain = "<DOMAIN>.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
        // Add and configure individual providers
        new GoogleProvider().AddScopes("email"),
        new EmailProvider()
                // ...
            },
            // WPF:
            UserRepository = new FileUserRepository("FirebaseSample") ,// persist data into %AppData%\FirebaseSample
    // UWP:
    UserRepository = new StorageRepository() // persist data into ApplicationDataContainer
        };

        public AuthService()
        {
            client = new FirebaseAuthClient(config);

            //_authProvider = new FirebaseAuthProvider(new FirebaseConfig("your-firebase-api-key"));
        }

        public Shared.Models.User? CurrentUser => _currentUser;

        public async Task<Shared.Models.User> Authenticate(string email, string password)
        {
            if (!email.EndsWith("@yourcompany.com", StringComparison.OrdinalIgnoreCase))
                //throw new AuthException("Invalid company domain");
                throw new Exception("Invalid company domain");

            var result = await client.SignInWithEmailAndPasswordAsync(email, password);
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
