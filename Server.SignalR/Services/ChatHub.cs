using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services
{
    public class ChatHub : Hub<Interfaces.IClientChatCallbacks>, Interfaces.IChatHub
    {
        private static readonly Dictionary<Guid, HashSet<Guid>> TypingStatus = new();
        private static readonly Dictionary<Guid, List<Entities.Dtos.MessageDto>> UnreadMessages = new();


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
        public async Task<List<Entities.Dtos.MessageDto>> GetUnreadMessages(Guid userId)
        {
            if (UnreadMessages.ContainsKey(userId))
            {
                return UnreadMessages[userId];
            }
            return new List<Entities.Dtos.MessageDto>();
        }

        public async Task AddUnreadMessage(Guid userId, Entities.Dtos.MessageDto message)
        {
            if (!UnreadMessages.ContainsKey(userId))
            {
                UnreadMessages[userId] = new List<Entities.Dtos.MessageDto>();
            }
            UnreadMessages[userId].Add(message);
        }

        public async Task Typing(Guid conversationId, Guid userId)
        {
            // Track typing status: conversationId -> userId
            if (!TypingStatus.ContainsKey(conversationId))
            {
                TypingStatus[conversationId] = new HashSet<Guid>();
            }

            TypingStatus[conversationId].Add(userId);

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userId, true);
        }

        public async Task StopTyping(Guid conversationId, Guid userId)
        {
            if (TypingStatus.ContainsKey(conversationId))
            {
                TypingStatus[conversationId].Remove(userId);

                // Broadcast typing status to the conversation.
                await _chatHubService.HandleTypingNotification(Context, conversationId, userId, false);
            }
        }

        public async Task GetTypingStatusForAllUsers(Guid conversationId)
        {
            // Send the current typing status for the conversation to the client.
            if (TypingStatus.ContainsKey(conversationId))
            {
                var typingUsers = TypingStatus[conversationId].ToList();

                // Broadcast typing status to the conversation.
                await _chatHubService.HandleTypingNotification(Context, conversationId, typingUsers);
            }
        }
    }
}
