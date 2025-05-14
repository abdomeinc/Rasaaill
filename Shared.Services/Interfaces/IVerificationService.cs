namespace Shared.Services.Interfaces
{
    public interface IVerificationService
    {
        Task SendVerificationCodeAsync(string email);
        Task<(bool Success, string? Error, Entities.Models.User? User)> VerifyCodeAsync(string email, string code);
    }
}
