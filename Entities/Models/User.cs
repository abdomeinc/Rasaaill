using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    /// <summary>
    /// Represents an application user with extended profile and authentication information.
    /// Inherits from IdentityUser with a Guid as the primary key.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL of the user's avatar image.
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was last modified.
        /// </summary>
        public DateTime? LastModifyDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is currently online.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last seen online.
        /// </summary>
        public DateTime? LastSeen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the list of conversation participants associated with the user.
        /// </summary>
        public virtual List<ConversationParticipant> Participants { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of messages sent by the user.
        /// </summary>
        public virtual List<Message> Messages { get; set; } = [];

        /// <summary>
        /// Gets or sets the refresh token for the user, used for authentication.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiry time of the refresh token.
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
