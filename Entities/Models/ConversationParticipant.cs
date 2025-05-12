namespace Entities.Models
{
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }

        public virtual Conversation Conversation { get; set; }

        public Guid ParticipantId { get; set; }

        public virtual User Participant { get; set; }
        public bool IsMuted { get; set; }
        public bool IsArchived { get; set; }
        public bool IsPinned { get; set; }
        public Guid? LastSeenMessageId { get; set; }
        public virtual Message? LastSeenMessage { get; set; }
    }
}
