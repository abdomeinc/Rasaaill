namespace Entities.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public required string EmailAddress { get; set; }

        public string? DisplayName { get; set; }

        public string? AvatarUrl { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? LastModifyDate { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastSeen { get; set; }

        public bool IsApproved { get; set; }

        public virtual List<ConversationParticipantDto> Participants { get; set; } = [];
        public virtual List<MessageDto> Messages { get; set; } = [];
        public List<string> Roles { get; set; } = new();
    }
}
