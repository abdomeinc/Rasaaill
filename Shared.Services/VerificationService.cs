using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Shared.Services
{
    /// <summary>
    /// Provides verification services for sending and validating email verification codes.
    /// </summary>
    public class VerificationService : Interfaces.IVerificationService
    {
        /// <summary>
        /// The email sender used to send verification codes.
        /// </summary>
        private readonly Interfaces.IEmailSender _emailSender;

        /// <summary>
        /// The store used to persist verification codes.
        /// </summary>
        private readonly Interfaces.IVerificationCodeStore _codeStore;

        /// <summary>
        /// The user manager for accessing user information.
        /// </summary>
        private readonly UserManager<Entities.Models.User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationService"/> class.
        /// </summary>
        /// <param name="emailSender">The email sender service.</param>
        /// <param name="codeStore">The verification code store.</param>
        /// <param name="userManager">The user manager.</param>
        public VerificationService(
            Interfaces.IEmailSender emailSender,
            Interfaces.IVerificationCodeStore codeStore,
            UserManager<Entities.Models.User> userManager)
        {
            _emailSender = emailSender;
            _codeStore = codeStore;
            _userManager = userManager;
        }

        /// <summary>
        /// Sends a 6-digit verification code to the specified email address.
        /// </summary>
        /// <param name="email">The email address to send the code to.</param>
        /// <returns>The generated verification code.</returns>
        public async Task<string> SendVerificationCodeAsync(string email)
        {
            // Generate 6-digit code
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            await _codeStore.StoreCodeAsync(email, code, TimeSpan.FromMinutes(5));

            var subject = "🔐 Your Login Verification Code";
            var body = $"<p>Your code is: <strong>{code}</strong><br/>It expires in 5 minutes.</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
            return code;
        }

        /// <summary>
        /// Verifies the provided code for the specified email address.
        /// </summary>
        /// <param name="email">The email address to verify.</param>
        /// <param name="code">The verification code to check.</param>
        /// <returns>
        /// A tuple indicating success, an error message if any, and the associated user if verification succeeds.
        /// </returns>
        public async Task<(bool Success, string? Error, Entities.Models.User? User)> VerifyCodeAsync(string email, string code)
        {
            Entities.Models.User? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, "Email not found.", null);
            }

            var storedCode = await _codeStore.GetCodeAsync(email);

            if (storedCode == null)
                return (false, "Verification code expired or not found.", null);

            if (!string.Equals(code, storedCode))
                return (false, "Invalid verification code.", null);

            // Optional: delete it immediately after successful validation
            await _codeStore.RemoveCodeAsync(email);

            return (true, null, user);
        }
    }
}
