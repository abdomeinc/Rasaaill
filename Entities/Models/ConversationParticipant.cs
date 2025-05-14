using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    /// <summary>
    /// Represents a participant in a conversation, including their state and preferences within the conversation.
    /// </summary>
    public class ConversationParticipant
    {
        /// <summary>
        /// Gets or sets the unique identifier for the conversation participant.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the conversation this participant belongs to.
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the conversation entity associated with this participant.
        /// </summary>
        public virtual Conversation Conversation { get; set; } = default!;

        /// <summary>
        /// Gets or sets the identifier of the user who is the participant.
        /// </summary>
        public Guid ParticipantId { get; set; }

        /// <summary>
        /// Gets or sets the user entity representing the participant.
        /// </summary>
        public virtual User Participant { get; set; }=default!;

        /// <summary>
        /// Gets or sets a value indicating whether the participant has muted the conversation.
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the participant has archived the conversation.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the participant has pinned the conversation.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the last message seen by the participant.
        /// </summary>
        public Guid? LastSeenMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last message entity seen by the participant.
        /// </summary>
        public virtual Message? LastSeenMessage { get; set; }

        /// <summary>
        /// Gets or sets the count of new messages for the participant in the conversation.
        /// </summary>
        public int NewMessagesCount { get; set; }
    }
}
