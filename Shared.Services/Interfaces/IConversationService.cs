namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving information about conversation participants.
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Retrieves the unique identifiers of all participants in a specified conversation.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a collection of participant GUIDs.
        /// </returns>
        Task<IEnumerable<Guid>> GetParticipants(Guid conversationId);

        /// <summary>
        /// Asynchronously retrieves the user IDs of all participants in a specified conversation.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a collection of user GUIDs.
        /// </returns>
        Task<IEnumerable<Guid>> GetParticipantUserIdsAsync(Guid conversationId);
    }
}
