namespace ChatClient.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Shared.Models.User> Authenticate(string email, string password);
        Task Logout();
        Shared.Models.User? CurrentUser { get; }
    }
}
