namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Defines a contract for sending emails asynchronously.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously to the specified recipient.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The HTML content of the email message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
