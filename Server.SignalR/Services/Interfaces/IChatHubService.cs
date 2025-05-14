using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR.Services.Interfaces
{
    /// <summary>
    /// Provides methods for handling chat hub operations such as sending messages, joining conversations,
    /// marking messages as seen, and managing typing notifications.
    /// </summary>
    public interface IChatHubService
    {
        /// <summary>
        /// Handles the logic for sending a message within a conversation.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="message">The message data transfer object containing message details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleSendMessage(HubCallerContext context, Entities.Dtos.MessageDto message);

        /// <summary>
        /// Handles the logic for a user joining a conversation.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="conversationId">The unique identifier of the conversation to join.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleJoinConversation(HubCallerContext context, Guid conversationId);

        /// <summary>
        /// Handles marking a specific message as seen by the user.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="messageId">The unique identifier of the message to mark as seen.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleMarkAsSeen(HubCallerContext context, Guid messageId);

        /// <summary>
        /// Handles sending a typing notification for a conversation.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleTypingNotification(HubCallerContext context, Guid conversationId);

        /// <summary>
        /// Handles sending a typing notification for a specific user in a conversation, with a specified state.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="state">The typing state (true if typing, false otherwise).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleTypingNotification(HubCallerContext context, Guid conversationId, Guid userId, bool state);

        /// <summary>
        /// Handles sending typing notifications for multiple users in a conversation.
        /// </summary>
        /// <param name="context">The context of the hub caller.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userIds">A list of user identifiers who are typing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleTypingNotification(HubCallerContext context, Guid conversationId, List<Guid> userIds);
    }
}
