using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services
{
    public class ChatHubService : Interfaces.IChatHubService
    {
        private readonly IHubContext<ChatHub, Interfaces.IClientChatCallbacks> _hubContext;

        private readonly Shared.Services.Interfaces.IUserPresenceService _presenceService;
        private readonly Shared.Services.Interfaces.IMessageService _messageService;
        private readonly Shared.Services.Interfaces.IConversationService _conversationService;

        public ChatHubService(
            IHubContext<ChatHub, Interfaces.IClientChatCallbacks> hubContext,
            Shared.Services.Interfaces.IUserPresenceService presenceService,
            Shared.Services.Interfaces.IMessageService messageService,
            Shared.Services.Interfaces.IConversationService conversationService)
        {
            _hubContext = hubContext;
            _presenceService = presenceService;
            _messageService = messageService;
            _conversationService = conversationService;
        }

        public async Task HandleSendMessage(HubCallerContext context, Entities.Dtos.MessageDto message)
        {
            var userId = _presenceService.GetUserIdByConnectionId(context.ConnectionId);
            var persistedMessage = await _messageService.SaveMessageAsync(message, userId);

            var recipients = await _conversationService.GetParticipants(message.ConversationId);
            foreach (var recipientId in recipients.Where(r => r != userId))
            {
                var connectionIds = _presenceService.GetConnectionsForUser(recipientId);
                foreach (var connectionId in connectionIds)
                    await _hubContext.Clients.Client(connectionId).ReceiveMessage(persistedMessage);

            }
        }

        public async Task HandleJoinConversation(HubCallerContext context, Guid conversationId)
        {
            var userId = _presenceService.GetUserIdByConnectionId(context.ConnectionId);

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

        public async Task HandleMarkAsSeen(HubCallerContext context, Guid messageId)
        {
            var userId = _presenceService.GetUserIdByConnectionId(context.ConnectionId);
            if (userId == Guid.Empty)
                return;

            var seenInfo = await _messageService.MarkMessageAsSeenAsync(messageId, userId);
            if (seenInfo == null)
                return;

            var senderConnections = _presenceService.GetConnectionsForUser(seenInfo.SenderUserId);
            foreach (var connId in senderConnections)
            {
                await _hubContext.Clients.Client(connId).MessageSeenNotification(seenInfo.ConversationId, messageId, userId);
            }
        }
        public async Task HandleTypingNotification(HubCallerContext context, Guid conversationId)
        {
            var userId = _presenceService.GetUserIdByConnectionId(context.ConnectionId);
            if (userId == Guid.Empty)
                return;

            var recipientConnections = await GetConnectionsForConversationParticipantsAsync(conversationId, excludeUserId: userId);
            foreach (var connectionId in recipientConnections)
            {
                await _hubContext.Clients.Client(connectionId).UserTyping(conversationId, userId);
            }
        }


        private async Task<IEnumerable<string>> GetConnectionsForConversationParticipantsAsync(Guid conversationId, Guid excludeUserId)
        {
            var participants = await _conversationService.GetParticipantUserIdsAsync(conversationId);
            var otherParticipantIds = participants.Where(id => id != excludeUserId);

            var allConnections = new List<string>();

            foreach (var userId in otherParticipantIds)
            {
                var userConnections = _presenceService.GetConnectionsForUser(userId);
                allConnections.AddRange(userConnections);
            }

            return allConnections;
        }
    }
}
