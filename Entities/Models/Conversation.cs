using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    /// <summary>
    /// Represents a conversation between users, which can be private or a group.
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the conversation.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the conversation (e.g., Private, Group).
        /// </summary>
        public Shared.ConversationType ConversationType { get; set; }

        /// <summary>
        /// Gets or sets the notification type for the conversation (e.g., All, Messages, Mute).
        /// </summary>
        public Shared.NotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the last message in the conversation.
        /// </summary>
        public Guid? LastMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last message in the conversation.
        /// </summary>
        public Message? LastMessage { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the conversation was created.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the list of messages in the conversation.
        /// Only the latest 50 messages are handled for performance optimization.
        /// </summary>
        public virtual List<Message> Messages { get; set; } = []; // will handle latest 50 messages for performance optimize

        /// <summary>
        /// Gets or sets the group name if the conversation type is Group.
        /// </summary>
        public string? GroupName { get; set; } // for conversation type Group

        /// <summary>
        /// Gets or sets the group icon URL if the conversation type is Group.
        /// </summary>
        public string? GroupIconUrl { get; set; } // for conversation type Group

        /// <summary>
        /// Gets or sets the list of participants in the conversation.
        /// </summary>
        public virtual List<ConversationParticipant> Participants { get; set; } = [];
    }
}
