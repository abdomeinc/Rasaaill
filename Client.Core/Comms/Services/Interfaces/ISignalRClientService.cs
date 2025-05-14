namespace Client.Core.Comms.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for a SignalR client service that manages real-time communication with a server.
    /// </summary>
    public interface ISignalRClientService
    {
        /// <summary>
        /// Establishes a connection to the SignalR server asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous connect operation.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the SignalR server asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sends a message to the SignalR server asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendMessageAsync(Entities.Dtos.MessageDto message);

        /// <summary>
        /// Joins a conversation group on the SignalR server asynchronously.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation to join.</param>
        /// <returns>A task representing the asynchronous join operation.</returns>
        Task JoinConversationAsync(Guid conversationId);
    }
}
