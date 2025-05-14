namespace Server.SignalR.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for chat hub operations, including sending messages, joining conversations,
    /// managing unread messages, and handling typing notifications in a SignalR-based chat system.
    /// </summary>
    public interface IChatHub
    {
        /// <summary>
        /// Sends a message asynchronously to the chat system.
        /// </summary>
        /// <param name="message">The message data transfer object containing message details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendMessageAsync(Entities.Dtos.MessageDto message);

        /// <summary>
        /// Joins the specified conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation to join.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task JoinConversationAsync(Guid conversationId);

        /// <summary>
        /// Marks the specified message as seen asynchronously.
        /// </summary>
        /// <param name="messageId">The unique identifier of the message to mark as seen.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MarkAsSeenAsync(Guid messageId);

        /// <summary>
        /// Notifies the chat system that a user is typing in the specified conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NotifyTypingAsync(Guid conversationId);

        /// <summary>
        /// Retrieves a list of unread messages for the specified user asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that returns a list of unread message DTOs for the user.</returns>
        Task<List<Entities.Dtos.MessageDto?>> GetUnreadMessages(Guid userId);

        /// <summary>
        /// Adds an unread message for the specified user asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="message">The message data transfer object to add as unread.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddUnreadMessage(Guid userId, Entities.Dtos.MessageDto message);

        /// <summary>
        /// Notifies the chat system that a user has started typing in the specified conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userId">The unique identifier of the user who is typing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Typing(Guid conversationId, Guid userId);

        /// <summary>
        /// Notifies the chat system that a user has stopped typing in the specified conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userId">The unique identifier of the user who stopped typing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StopTyping(Guid conversationId, Guid userId);

        /// <summary>
        /// Retrieves the typing status for all users in the specified conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GetTypingStatusForAllUsers(Guid conversationId);
    }
}
