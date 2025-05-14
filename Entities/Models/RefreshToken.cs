namespace Entities.Models
{
    /// <summary>
    /// Represents a refresh token used for renewing JWT access tokens.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for the refresh token.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the token string value.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expiration date and time of the refresh token.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the refresh token has been revoked.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Gets or sets the identifier of the associated JWT access token.
        /// </summary>
        public string JwtId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user to whom this refresh token belongs.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user entity associated with this refresh token.
        /// </summary>
        public User User { get; set; } = default!;
    }
}
