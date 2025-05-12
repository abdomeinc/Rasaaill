using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services
{
    public class ChatHub : Hub<Interfaces.IClientChatCallbacks>, Interfaces.IChatHub
    {
        private readonly Interfaces.IChatHubService _chatHubService;

        public ChatHub(Interfaces.IChatHubService chatHubService)
        {
            _chatHubService = chatHubService;
        }

        public async Task SendMessageAsync(Entities.Dtos.MessageDto message) =>
            await _chatHubService.HandleSendMessage(Context, message);

        public async Task JoinConversationAsync(Guid conversationId) =>
            await _chatHubService.HandleJoinConversation(Context, conversationId);

        public async Task MarkAsSeenAsync(Guid messageId) =>
            await _chatHubService.HandleMarkAsSeen(Context, messageId);

        public async Task NotifyTypingAsync(Guid conversationId) =>
            await _chatHubService.HandleTypingNotification(Context, conversationId);
    }
}
