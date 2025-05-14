using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace Server.SignalR.Services
{
    /// <summary>
    /// SignalR hub for managing chat operations such as sending messages, joining conversations, typing notifications, and unread message management.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub<Interfaces.IClientChatCallbacks>, Interfaces.IChatHub
    {
        /// <summary>
        /// Redis database instance for storing chat-related data such as unread messages and typing status.
        /// </summary>
        private readonly IDatabase _redis;

        /// <summary>
        /// Service for handling chat hub business logic.
        /// </summary>
        private readonly Interfaces.IChatHubService _chatHubService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHub"/> class.
        /// </summary>
        /// <param name="redis">Redis connection multiplexer.</param>
        /// <param name="chatHubService">Chat hub service for business logic.</param>
        public ChatHub(IConnectionMultiplexer redis, Interfaces.IChatHubService chatHubService)
        {
            _chatHubService = chatHubService;
            _redis = redis.GetDatabase();
        }

        /// <summary>
        /// Sends a message to the conversation.
        /// </summary>
        /// <param name="message">The message DTO to send.</param>
        public async Task SendMessageAsync(Entities.Dtos.MessageDto message)
        {
            await _chatHubService.HandleSendMessage(Context, message);
        }

        /// <summary>
        /// Joins the current user to a conversation.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
        public async Task JoinConversationAsync(Guid conversationId)
        {
            await _chatHubService.HandleJoinConversation(Context, conversationId);
        }

        /// <summary>
        /// Marks a message as seen by the current user.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        public async Task MarkAsSeenAsync(Guid messageId)
        {
            await _chatHubService.HandleMarkAsSeen(Context, messageId);
        }

        /// <summary>
        /// Notifies others that the current user is typing in a conversation.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
        public async Task NotifyTypingAsync(Guid conversationId)
        {
            await _chatHubService.HandleTypingNotification(Context, conversationId);
        }

        /// <summary>
        /// Retrieves all unread messages for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>List of unread message DTOs.</returns>
        public async Task<List<Entities.Dtos.MessageDto?>> GetUnreadMessages(Guid userId)
        {
            // Read all unread messages
            var entries = await _redis.ListRangeAsync($"unread:{userId}");
            var messages = entries
                .Where(entry => !entry.IsNull) // Filter out null RedisValue entries
                .Select(entry => JsonSerializer.Deserialize<Entities.Dtos.MessageDto>(entry.ToString())).ToList();

            return messages ?? [];
        }

        /// <summary>
        /// Adds an unread message for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message DTO to add.</param>
        public async Task AddUnreadMessage(Guid userId, Entities.Dtos.MessageDto message)
        {
            await _redis.ListRightPushAsync($"unread:{userId}", JsonSerializer.Serialize(message));
        }

        /// <summary>
        /// Marks the user as typing in a conversation and broadcasts the typing status.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public async Task Typing(Guid conversationId, Guid userId)
        {
            await _redis.SetAddAsync($"typing:{conversationId}", userId.ToString());

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userId, true);
        }

        /// <summary>
        /// Marks the user as stopped typing in a conversation and broadcasts the typing status.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public async Task StopTyping(Guid conversationId, Guid userId)
        {
            await _redis.SetRemoveAsync($"typing:{conversationId}", userId.ToString());

            // Broadcast typing status to the conversation.
            await _chatHubService.HandleTypingNotification(Context, conversationId, userId, false);
        }

        /// <summary>
        /// Gets the typing status for all users in a conversation and broadcasts it.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
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
