namespace Entities.Models
{
    public class User
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
    }
}
