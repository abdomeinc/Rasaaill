namespace Entities.Models
{
    /// <summary>
    /// Represents an email verification entity containing the verification code, email, and expiry information.
    /// </summary>
    public class EmailVerification
    {
        /// <summary>
        /// Gets or sets the unique identifier for the email verification entry.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the email address to be verified.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verification code sent to the email address.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiry date and time for the verification code.
        /// </summary>
        public DateTime Expiry { get; set; }
    }
}
