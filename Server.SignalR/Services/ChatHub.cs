using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace Server.SignalR.Services
{
    public class ChatHub : Hub<Interfaces.IClientChatCallbacks>, Interfaces.IChatHub
    {
        private readonly IDatabase _redis;

        private readonly Interfaces.IChatHubService _chatHubService;

        public ChatHub(IConnectionMultiplexer redis, Interfaces.IChatHubService chatHubService)
        {
            _chatHubService = chatHubService;
            _redis = redis.GetDatabase();

        }

        public async Task SendMessageAsync(Entities.Dtos.MessageDto message)
        {
            await _chatHubService.HandleSendMessage(Context, message);
        }

        public async Task JoinConversationAsync(Guid conversationId)
        {
            await _chatHubService.HandleJoinConversation(Context, conversationId);
        }

        public async Task MarkAsSeenAsync(Guid messageId)
        {
            await _chatHubService.HandleMarkAsSeen(Context, messageId);
        }

        public async Task NotifyTypingAsync(Guid conversationId)
        {
            await _chatHubService.HandleTypingNotification(Context, conversationId);
        }

        public async Task<List<Entities.Dtos.MessageDto?>> GetUnreadMessages(Guid userId)
        {
            // Read all unread messages
            var entries = await _redis.ListRangeAsync($"unread:{userId}");
            var messages = entries
                .Where(entry => !entry.IsNull) // Filter out null RedisValue entries
                .Select(entry => JsonSerializer.Deserialize<Entities.Dtos.MessageDto>(entry.ToString())).ToList();

            return messages ?? [];
        }

        public async Task AddUnreadMessage(Guid userId, Entities.Dtos.MessageDto message)
        {
            await _redis.ListRightPushAsync($"unread:{userId}", JsonSerializer.Serialize(message));

        }

        public async Task Typing(Guid conversationId, Guid userId)
        {
            await _redis.SetAddAsync($"typing:{conversationId}", userId.ToString());

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userId, true);
        }

        public async Task StopTyping(Guid conversationId, Guid userId)
        {
            await _redis.SetRemoveAsync($"typing:{conversationId}", userId.ToString());

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userId, false);
        }

        public async Task GetTypingStatusForAllUsers(Guid conversationId)
        {
            // Get current typers
            var userIdsRedisValue = await _redis.SetMembersAsync($"typing:{conversationId}");
            if (userIdsRedisValue == null || userIdsRedisValue.Length == 0)
            {
                // No users are typing
                await _chatHubService.HandleTypingNotification(Context, conversationId, new List<Guid>());
                return;
            }

            // Convert RedisValue[] to List<Guid> by converting to string and then parsing as Guid
            var userIds = userIdsRedisValue
                .Select(rv => Guid.Parse(rv.ToString())) // Convert to string and then parse as Guid
                .ToList();

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userIds);
        }
    }
}
