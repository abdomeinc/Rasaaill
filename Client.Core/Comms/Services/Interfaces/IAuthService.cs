namespace Client.Core.Comms.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email);
        Task LogoutAsync();

        bool IsAuthenticated { get; }
        Entities.Dtos.UserDto? CurrentUser { get; }

        string? AccessToken { get; }
        Task<bool> TryAutoLoginAsync();


        Task<bool> RequestVerificationCodeAsync(string email);
        Task<bool> VerifyCodeAndLoginAsync(string email, string code);
        Task<bool> ValidateTokenAsync(string token);
        Task<string?> GetStoredTokenAsync();

    }
}
