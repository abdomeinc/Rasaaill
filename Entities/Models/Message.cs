using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    /// <summary>
    /// Represents a message within a conversation, including its content, sender, state, and media information.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the state of the message (e.g., Unknown, Failed, Recalled, Deleted, Sending, Sent, Received, Seen).
        /// </summary>
        public Shared.MessageState State { get; set; }

        /// <summary>
        /// Gets or sets the type of the message (e.g., Text, Image, Video, Document).
        /// </summary>
        public Shared.MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the conversation this message belongs to.
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the conversation entity associated with this message.
        /// </summary>
        public virtual Conversation Conversation { get; set; } = default!;

        /// <summary>
        /// Gets or sets the identifier of the user who sent the message.
        /// </summary>
        public Guid SenderId { get; set; }

        /// <summary>
        /// Gets or sets the user entity representing the sender of the message.
        /// </summary>
        public virtual User Sender { get; set; } = default!;

        /// <summary>
        /// Gets or sets the textual content of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the message was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the message was last edited, if applicable.
        /// </summary>
        public DateTime? EditTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the message this message is replying to, if any.
        /// </summary>
        public Guid? ReplyToMessageId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the message this message was forwarded from, if any.
        /// </summary>
        public Guid? ForwardedFromMessageId { get; set; }

        /// <summary>
        /// Gets or sets the file name for media messages (Image, Video, or Document).
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the local file path for the media file. Not mapped to the database.
        /// </summary>
        [NotMapped]
        public string LocalFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the media file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the file preview as a byte array. Not mapped to the database.
        /// </summary>
        [NotMapped]
        public byte[] FilePreview { get; set; } = [];

        /// <summary>
        /// Gets or sets the cloud storage URL for the media file.
        /// </summary>
        public string CloudUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cloud storage URL for the media file preview.
        /// </summary>
        public string CloudFilePreviewUrl { get; set; } = string.Empty;
    }
}
