using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client.WPF.ViewModels
{
    public class MainViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly Core.Comms.Services.Interfaces.ITokenStorageService _tokenStorage;
        private readonly Core.Comms.Services.Interfaces.IAuthService _authService;

        public MainViewModel(HttpClient httpClient, Core.Comms.Services.Interfaces.ITokenStorageService tokenStorage, Core.Comms.Services.Interfaces.IAuthService authService)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
            _authService = authService;

            Initialize();
        }

        private async void Initialize()
        {
            var token = _tokenStorage.Load();
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Set Authorization header globally or on HttpClient
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var isValid = await _authService.ValidateTokenAsync(token); // optional, depends on server support
                if (isValid)
                {
                    NavigateToMainChat();
                    return;
                }
            }
            NavigateToLogin();
        }

        private void NavigateToLogin()
        {
            throw new NotImplementedException();
        }

        private void NavigateToMainChat()
        {
            throw new NotImplementedException();
        }
    }
}
