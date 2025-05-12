namespace Server.SignalR.Services.Interfaces
{
    public interface IChatHub
    {
        Task SendMessageAsync(Entities.Dtos.MessageDto message);
        Task JoinConversationAsync(Guid conversationId);
        Task MarkAsSeenAsync(Guid messageId);
        Task NotifyTypingAsync(Guid conversationId);
    }
}
