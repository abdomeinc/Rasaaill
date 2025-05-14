using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services
{
    /// <summary>
    /// Service for handling chat hub business logic, including sending messages, joining conversations,
    /// marking messages as seen, and managing typing notifications.
    /// </summary>
    public class ChatHubService : Interfaces.IChatHubService
    {
        /// <summary>
        /// SignalR hub context for sending messages to clients.
        /// </summary>
        private readonly IHubContext<ChatHub, Interfaces.IClientChatCallbacks> _hubContext;

        /// <summary>
        /// Logger for logging information and errors.
        /// </summary>
        private readonly ILogger<ChatHubService> _logger;

        /// <summary>
        /// Service for managing user presence and connections.
        /// </summary>
        private readonly Shared.Services.Interfaces.IUserPresenceService _presenceService;

        /// <summary>
        /// Service for handling message persistence and state.
        /// </summary>
        private readonly Shared.Services.Interfaces.IMessageService _messageService;

        /// <summary>
        /// Service for managing conversation participants and details.
        /// </summary>
        private readonly Shared.Services.Interfaces.IConversationService _conversationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHubService"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="hubContext">SignalR hub context.</param>
        /// <param name="presenceService">User presence service.</param>
        /// <param name="messageService">Message service.</param>
        /// <param name="conversationService">Conversation service.</param>
        public ChatHubService(
            ILogger<ChatHubService> logger,
            IHubContext<ChatHub, Interfaces.IClientChatCallbacks> hubContext,
            Shared.Services.Interfaces.IUserPresenceService presenceService,
            Shared.Services.Interfaces.IMessageService messageService,
            Shared.Services.Interfaces.IConversationService conversationService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _presenceService = presenceService;
            _messageService = messageService;
            _conversationService = conversationService;
        }

        /// <summary>
        /// Handles sending a message from a user to all participants in a conversation.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="message">Message DTO to send.</param>
        public async Task HandleSendMessage(HubCallerContext context, Entities.Dtos.MessageDto message)
        {
            try
            {
                var userId = await _presenceService.GetUserIdByConnectionIdAsync(context.ConnectionId);

                if (userId == Guid.Empty)
                    return;

                _logger.LogInformation("User {UserId} sent a message", userId);

                var persistedMessage = await _messageService.SaveMessageAsync(message, userId);

                var recipients = await _conversationService.GetParticipants(message.ConversationId);
                foreach (var recipientId in recipients.Where(r => r != userId))
                {
                    var connectionIds = await _presenceService.GetConnectionsForUserAsync(recipientId);
                    foreach (var connectionId in connectionIds)
                        await _hubContext.Clients.Client(connectionId).ReceiveMessage(persistedMessage);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleSendMessage: {@Message}", message);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Handles a user joining a conversation, adds them to the SignalR group, and notifies other participants.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="conversationId">Conversation identifier.</param>
        public async Task HandleJoinConversation(HubCallerContext context, Guid conversationId)
        {
            try
            {
                var userId = await _presenceService.GetUserIdByConnectionIdAsync(context.ConnectionId);

                if (userId == Guid.Empty)
                    return;

                // Add this connection to a SignalR group named after the conversation ID
                await _hubContext.Groups.AddToGroupAsync(context.ConnectionId, conversationId.ToString());

                // Optional: Notify others that this user has joined
                var connectionIds = await GetConnectionsForConversationParticipantsAsync(conversationId, excludeUserId: userId);
                foreach (var connectionId in connectionIds)
                {
                    await _hubContext.Clients.Client(connectionId).UserJoinedConversation(userId, conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleJoinConversation: {@Conversation Id}", conversationId);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Handles marking a message as seen by the current user and notifies the sender.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="messageId">Message identifier.</param>
        public async Task HandleMarkAsSeen(HubCallerContext context, Guid messageId)
        {
            try
            {
                var userId = await _presenceService.GetUserIdByConnectionIdAsync(context.ConnectionId);
                if (userId == Guid.Empty)
                    return;

                var seenInfo = await _messageService.MarkMessageAsSeenAsync(messageId, userId);
                if (seenInfo == null)
                    return;

                var senderConnections = await _presenceService.GetConnectionsForUserAsync(seenInfo.SenderUserId);
                foreach (var connId in senderConnections)
                {
                    await _hubContext.Clients.Client(connId).MessageSeenNotification(seenInfo.ConversationId, messageId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleMarkAsSeen: {@Message Id}", messageId);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Handles notifying other participants that the current user is typing in a conversation.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="conversationId">Conversation identifier.</param>
        public async Task HandleTypingNotification(HubCallerContext context, Guid conversationId)
        {
            try
            {
                var userId = await _presenceService.GetUserIdByConnectionIdAsync(context.ConnectionId);
                if (userId == Guid.Empty)
                    return;

                var recipientConnections = await GetConnectionsForConversationParticipantsAsync(conversationId, excludeUserId: userId);
                foreach (var connectionId in recipientConnections)
                {
                    await _hubContext.Clients.Client(connectionId).UserTyping(conversationId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleTypingNotification: {@Conversation Id}", conversationId);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Handles notifying other participants that a specific user is typing or stopped typing in a conversation.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="conversationId">Conversation identifier.</param>
        /// <param name="userId">User identifier.</param>
        /// <param name="state">Typing state (true if typing, false if stopped).</param>
        public async Task HandleTypingNotification(HubCallerContext context, Guid conversationId, Guid userId, bool state)
        {
            try
            {
                var recipientConnections = await GetConnectionsForConversationParticipantsAsync(conversationId, excludeUserId: userId);
                foreach (var connectionId in recipientConnections)
                {
                    await _hubContext.Clients.Client(connectionId).UserTyping(conversationId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleTypingNotification: {@Conversation Id}", conversationId);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Handles notifying other participants that multiple users are typing in a conversation.
        /// </summary>
        /// <param name="context">Hub caller context.</param>
        /// <param name="conversationId">Conversation identifier.</param>
        /// <param name="userIds">List of user identifiers who are typing.</param>
        public async Task HandleTypingNotification(HubCallerContext context, Guid conversationId, List<Guid> userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var recipientConnections = await GetConnectionsForConversationParticipantsAsync(conversationId, excludeUserId: userId);
                    foreach (var connectionId in recipientConnections)
                    {
                        await _hubContext.Clients.Client(connectionId).UserTyping(conversationId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleTypingNotification: {@Conversation Id}", conversationId);
                // Optionally notify the sender
            }
        }

        /// <summary>
        /// Gets all connection IDs for participants in a conversation, excluding a specific user.
        /// </summary>
        /// <param name="conversationId">Conversation identifier.</param>
        /// <param name="excludeUserId">User identifier to exclude from the result.</param>
        /// <returns>Enumerable of connection IDs.</returns>
        private async Task<IEnumerable<string>> GetConnectionsForConversationParticipantsAsync(Guid conversationId, Guid excludeUserId)
        {
            try
            {
                var participants = await _conversationService.GetParticipantUserIdsAsync(conversationId);
                var otherParticipantIds = participants.Where(id => id != excludeUserId);

                var allConnections = new List<string>();

                foreach (var userId in otherParticipantIds)
                {
                    var userConnections = await _presenceService.GetConnectionsForUserAsync(userId);
                    allConnections.AddRange(userConnections);
                }

                return allConnections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetConnectionsForConversationParticipantsAsync: {@conversationId}", conversationId);
                // Optionally notify the sender
                return [];
            }
        }
    }
}
