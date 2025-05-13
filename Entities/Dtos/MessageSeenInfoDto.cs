namespace Entities.Dtos
{
    public class MessageSeenInfoDto
    {
        public Guid ConversationId { get; set; }
        public Guid SenderUserId { get; set; }
    }
}
