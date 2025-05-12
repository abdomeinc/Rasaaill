using LiteDB;

namespace Shared.Models
{
    public class Message
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Guid? GroupId { get; set; }
        public Guid? ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Enums.MessageType Type { get; set; }
        public Enums.MessageStatus Status { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
