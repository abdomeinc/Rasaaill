using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Shared.Services
{
    /// <summary>
    /// Provides functionality to send emails using the SendGrid service.
    /// </summary>
    public class SendGridEmailSender : Interfaces.IEmailSender
    {
        /// <summary>
        /// The application configuration instance used to retrieve SendGrid settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The logger instance for logging email sending operations.
        /// </summary>
        private readonly ILogger<SendGridEmailSender> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmailSender"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger for this class.</param>
        public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email asynchronously using SendGrid.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
