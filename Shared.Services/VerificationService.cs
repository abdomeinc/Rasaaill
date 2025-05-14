using Microsoft.AspNetCore.Identity;

namespace Shared.Services
{
    public class VerificationService : Interfaces.IVerificationService
    {
        private readonly Interfaces.IEmailSender _emailSender;
        private readonly Interfaces.IVerificationCodeStore _codeStore;
        private readonly UserManager<Entities.Models.User> _userManager;

        public VerificationService(Interfaces.IEmailSender emailSender, Interfaces.IVerificationCodeStore codeStore, UserManager<Entities.Models.User> userManager)
        {
            _emailSender = emailSender;
            _codeStore = codeStore;
            _userManager = userManager;
        }

        public async Task SendVerificationCodeAsync(string email)
        {
            var code = Random.Shared.Next(100_000, 999_999).ToString();
            await _codeStore.StoreCodeAsync(email, code, TimeSpan.FromMinutes(5));

            var subject = "🔐 Your Login Verification Code";
            var body = $"<p>Your code is: <strong>{code}</strong><br/>It expires in 5 minutes.</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }

        public async Task<(bool Success, string? Error, Entities.Models.User? User)> VerifyCodeAsync(string email, string code)
        {
            var storedCode = await _codeStore.GetCodeAsync(email);
            if (storedCode == null)
                return (false, "Verification code expired or not found.", null);

            if (!string.Equals(code, storedCode))
                return (false, "Invalid verification code.", null);

            // Optional: delete it immediately after successful validation
            await _codeStore.RemoveCodeAsync(email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new Entities.Models.User { Email = email, UserName = email };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return (false, "Failed to create user.", null);
            }

            return (true, null, user);
        }

    }
}
