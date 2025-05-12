
namespace Server.SignalR.Services.Interfaces
{
    public interface IClientChatCallbacks
    {
        Task ReceiveMessage(Entities.Dtos.MessageDto message);
        Task UserJoined(string userId);
        Task UserLeft(string userId);
        Task ConversationUpdated(Entities.Dtos.ConversationDto conversation);
        Task UserJoinedConversation(Guid userId, Guid conversationId);
        Task MessageSeenNotification(Guid conversationId, Guid messageId, Guid userId);
        Task UserTyping(Guid conversationId, Guid userId);
    }
}
