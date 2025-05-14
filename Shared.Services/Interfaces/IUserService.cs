namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing user-related operations such as setting online status.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Sets the online status of a user asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="isOnline">A boolean value indicating whether the user is online.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetUserOnlineStatusAsync(Guid userId, bool isOnline);
    }
}
