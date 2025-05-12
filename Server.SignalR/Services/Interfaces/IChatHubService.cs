using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services.Interfaces
{
    public interface IChatHubService
    {
        Task HandleSendMessage(HubCallerContext context, Entities.Dtos.MessageDto message);
        Task HandleJoinConversation(HubCallerContext context, Guid conversationId);
        Task HandleMarkAsSeen(HubCallerContext context, Guid messageId);
        Task HandleTypingNotification(HubCallerContext context, Guid conversationId);
    }
}
