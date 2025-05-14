using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    public class LocalEmailSender:Interfaces.IEmailSender
    {
        private readonly ILogger<LocalEmailSender> _logger;

        public LocalEmailSender(ILogger<LocalEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            _logger.LogInformation("📧 Email to {To}: {Subject}\n{Body}", toEmail, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}
