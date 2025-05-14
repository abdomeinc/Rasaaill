
namespace Server.SignalR.Services.Interfaces
{
    /// <summary>
    /// Defines callback methods that the server can invoke on connected chat clients.
    /// </summary>
    public interface IClientChatCallbacks
    {
        /// <summary>
        /// Notifies the client that a new message has been received in a conversation.
        /// </summary>
        /// <param name="message">The message data transfer object containing message details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ReceiveMessage(Entities.Dtos.MessageDto message);

        /// <summary>
        /// Notifies the client that a user has joined the chat system.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who joined.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UserJoined(string userId);

        /// <summary>
        /// Notifies the client that a user has left the chat system.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who left.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UserLeft(string userId);

        /// <summary>
        /// Notifies the client that a conversation has been updated (e.g., new message, name change).
        /// </summary>
        /// <param name="conversation">The conversation data transfer object containing updated details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConversationUpdated(Entities.Dtos.ConversationDto conversation);

        /// <summary>
        /// Notifies the client that a user has joined a specific conversation.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who joined.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UserJoinedConversation(Guid userId, Guid conversationId);

        /// <summary>
        /// Notifies the client that a message in a conversation has been seen by a user.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="messageId">The unique identifier of the message that was seen.</param>
        /// <param name="userId">The unique identifier of the user who saw the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MessageSeenNotification(Guid conversationId, Guid messageId, Guid userId);

        /// <summary>
        /// Notifies the client that a user is typing in a conversation.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userId">The unique identifier of the user who is typing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UserTyping(Guid conversationId, Guid userId);
    }
}
