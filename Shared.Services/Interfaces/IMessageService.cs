namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing messages, including saving and marking messages as seen.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Marks the specified message as seen by the given user.
        /// </summary>
        /// <param name="messageId">The unique identifier of the message to mark as seen.</param>
        /// <param name="userId">The unique identifier of the user who has seen the message.</param>
        /// <returns>
        /// A <see cref="Entities.Dtos.MessageSeenInfoDto"/> containing information about the seen message.
        /// </returns>
        Task<Entities.Dtos.MessageSeenInfoDto> MarkMessageAsSeenAsync(Guid messageId, Guid userId);

        /// <summary>
        /// Saves a new message or updates an existing message for the specified user.
        /// </summary>
        /// <param name="message">The message data to save.</param>
        /// <param name="userId">The unique identifier of the user saving the message.</param>
        /// <returns>
        /// A <see cref="Entities.Dtos.MessageDto"/> representing the saved message.
        /// </returns>
        Task<Entities.Dtos.MessageDto> SaveMessageAsync(Entities.Dtos.MessageDto message, Guid userId);
    }
}
