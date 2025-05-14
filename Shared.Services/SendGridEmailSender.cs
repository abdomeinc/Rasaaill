using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Shared.Services
{
    public class SendGridEmailSender : Interfaces.IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlMessage);
            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("Sent email to {To} with status {StatusCode}", toEmail, response.StatusCode);
        }
    }
}
