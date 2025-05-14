using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    /// <summary>
    /// Provides a local implementation of the <see cref="Interfaces.IEmailSender"/> interface
    /// that logs email details instead of sending actual emails.
    /// </summary>
    public class LocalEmailSender : Interfaces.IEmailSender
    {
        /// <summary>
        /// The logger instance used to log email information.
        /// </summary>
        private readonly ILogger<LocalEmailSender> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalEmailSender"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging email information.</param>
        public LocalEmailSender(ILogger<LocalEmailSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs the email details instead of sending an actual email.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email.</param>
        /// <returns>A completed task.</returns>
        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            _logger.LogInformation("📧 Email to {To}: {Subject}\n{Body}", toEmail, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}
