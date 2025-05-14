namespace Client.Core.Comms.Services.Interfaces
{
    /// <summary>
    /// Defines methods for handling client-side messaging events such as receiving messages, typing notifications, and message seen updates.
    /// </summary>
    public interface IClientMessageHandler
    {
        /// <summary>
        /// Invoked when a new message is received.
        /// </summary>
        /// <param name="message">The message data transfer object containing message details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnMessageReceived(Entities.Dtos.MessageDto message);

        /// <summary>
        /// Invoked when a user is typing in a conversation.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation where typing is occurring.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnTyping(Guid conversationId);

        /// <summary>
        /// Invoked when a message has been seen by the recipient.
        /// </summary>
        /// <param name="messageId">The unique identifier of the message that was seen.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnMessageSeen(Guid messageId);

        /// <summary>
        /// Invoked to update the typing status of users in a conversation.
        /// </summary>
        /// <param name="typingUsers">A list of user identifiers who are currently typing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnUpdateTypingStatus(List<Guid> typingUsers);
    }
}
