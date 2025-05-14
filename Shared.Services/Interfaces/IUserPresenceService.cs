namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing user presence and their active connections.
    /// </summary>
    public interface IUserPresenceService
    {
        /// <summary>
        /// Adds a new connection for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="connectionId">The unique identifier of the connection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddConnectionAsync(Guid userId, string connectionId);

        /// <summary>
        /// Removes a connection by its identifier.
        /// </summary>
        /// <param name="connectionId">The unique identifier of the connection to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveConnectionAsync(string connectionId);

        /// <summary>
        /// Gets all connection identifiers associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that returns a collection of connection identifiers.</returns>
        Task<IEnumerable<string>> GetConnectionsForUserAsync(Guid userId);

        /// <summary>
        /// Gets the user identifier associated with a specific connection identifier.
        /// </summary>
        /// <param name="connectionId">The unique identifier of the connection.</param>
        /// <returns>A task that returns the user identifier.</returns>
        Task<Guid> GetUserIdByConnectionIdAsync(string connectionId);
    }
}
