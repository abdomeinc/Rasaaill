namespace Entities.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        public Shared.Enums.MessageState State { get; set; } //  Unknown (default fallback), Failed (for sending errors), Recalled (for undo message feature), Deleted (soft delete "for hiding from one side"), Sending (just created), local only, Sent (server accepted), Received (receiver got it), Seen (receiver opened it)

        public Shared.Enums.MessageType Type { get; set; } // Text, Image, Video, Document

        public Guid ConversationId { get; set; }

        public virtual Conversation Conversation { get; set; }

        public Guid SenderId { get; set; }

        public virtual User Sender { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? FilePath { get; set; } // for image, video or document message type

        public long? FileSize { get; set; } // for image, video or document message type

        public byte[]? FilePreview { get; set; } // for image, video or document message type

        public string? LocalFilePath { get; set; } // for image, video or document message type

        public string? CloudUrl { get; set; } // for image, video or document message type

        public string? CloudFilePreviewUrl { get; set; } // for image, video or document message type

        public DateTime Timestamp { get; set; }
        public DateTime? EditTimestamp { get; set; }

        public Guid? ReplyToMessageId { get; set; }

        public Guid? ForwardedFromMessageId { get; set; }

    }
}
