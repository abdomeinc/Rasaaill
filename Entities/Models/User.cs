using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    public class User : IdentityUser<Guid>
    {
        //public Guid Id { get; set; }

        //public required string EmailAddress { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        //public string? PhoneNumber { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? LastModifyDate { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastSeen { get; set; }

        public bool IsApproved { get; set; }

        public virtual List<ConversationParticipant> Participants { get; set; } = [];
        public virtual List<Message> Messages { get; set; } = [];
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
