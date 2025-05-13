namespace Entities.Dtos
{
    public class ConversationParticipantDto
    {
        public Guid ConversationId { get; set; }

        public virtual ConversationDto Conversation { get; set; }

        public Guid ParticipantId { get; set; }

        public virtual UserDto Participant { get; set; }

        public bool IsMuted { get; set; }

        public bool IsArchived { get; set; }

        public bool IsPinned { get; set; }

        public Guid? LastSeenMessageId { get; set; }

        public virtual MessageDto? LastSeenMessage { get; set; }
    }
}
