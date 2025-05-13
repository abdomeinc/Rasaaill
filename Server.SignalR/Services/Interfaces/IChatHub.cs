namespace Server.SignalR.Services.Interfaces
{
    public interface IChatHub
    {
        Task SendMessageAsync(Entities.Dtos.MessageDto message);
        Task JoinConversationAsync(Guid conversationId);
        Task MarkAsSeenAsync(Guid messageId);
        Task NotifyTypingAsync(Guid conversationId);
        Task<List<Entities.Dtos.MessageDto?>> GetUnreadMessages(Guid userId);
        Task AddUnreadMessage(Guid userId, Entities.Dtos.MessageDto message);
        Task Typing(Guid conversationId, Guid userId);
        Task StopTyping(Guid conversationId, Guid userId);
        Task GetTypingStatusForAllUsers(Guid conversationId);
    }
}
