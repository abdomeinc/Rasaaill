namespace Entities.Dtos
{
    public class ConversationDto
    {
        public Guid Id { get; set; }

        public Shared.ConversationType ConversationType { get; set; }

        public Shared.NotificationType NotificationType { get; set; }

        public MessageDto LastMessage { get; set; } = default!;

        public DateTime CreationDate { get; set; }

        public virtual List<MessageDto> Messages { get; set; } = []; // will handle latest 50 messages for performance optimize

        public string? GroupName { get; set; } // for conversation type Group

        public string? GroupIconUrl { get; set; } // for conversation type Group
    }
}
