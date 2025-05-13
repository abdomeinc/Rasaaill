namespace Entities.Dtos
{
    public class MessageDto
    {
        public Guid Id { get; set; }

        public Shared.MessageState State { get; set; } //  Unknown (default fallback), Failed (for sending errors), Recalled (for undo message feature), Deleted (soft delete "for hiding from one side"), Sending (just created), local only, Sent (server accepted), Received (receiver got it), Seen (receiver opened it)

        public Shared.MessageType Type { get; set; } // Text, Image, Video, Document

        public Guid ConversationId { get; set; }

        public Guid SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public DateTime? EditTimestamp { get; set; }

        public Guid? ReplyToMessageId { get; set; }

        public Guid? ForwardedFromMessageId { get; set; }

        public bool IsMedia => Type is Shared.MessageType.Image or Shared.MessageType.Video or Shared.MessageType.Document;

        // For media (Image, Video or Document) message
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string CloudUrl { get; set; } = string.Empty;

        public string CloudFilePreviewUrl { get; set; } = string.Empty;
    }
}
