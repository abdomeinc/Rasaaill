using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Shared.Services
{
    /// <summary>
    /// Service for tracking user presence using Redis.
    /// Manages the mapping between users and their active SignalR connection IDs,
    /// and updates user online status accordingly.
    /// </summary>
    public class UserPresenceService : Interfaces.IUserPresenceService
    {
        /// <summary>
        /// The Redis database instance used for presence tracking.
        /// </summary>
        private readonly IDatabase _redis;

        /// <summary>
        /// Logger for logging information and errors.
        /// </summary>
        private readonly ILogger<UserPresenceService> _logger;

        /// <summary>
        /// Service for conversation-related operations.
        /// </summary>
        private readonly Interfaces.IConversationService _conversationService;

        /// <summary>
        /// Service for user-related operations, such as updating online status.
        /// </summary>
        private readonly Interfaces.IUserService _userService;

        /// <summary>
        /// Expiration time for presence keys in Redis (default: 5 minutes).
        /// </summary>
        private static readonly TimeSpan PresenceKeyExpiration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPresenceService"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="conversationService">Conversation service instance.</param>
        /// <param name="redis">Redis connection multiplexer.</param>
        /// <param name="userService">User service instance.</param>
        public UserPresenceService(
            ILogger<UserPresenceService> logger,
            Interfaces.IConversationService conversationService,
            IConnectionMultiplexer redis,
            Interfaces.IUserService userService)
        {
            _logger = logger;
            _conversationService = conversationService;
            _userService = userService;
            _redis = redis.GetDatabase();
        }

        /// <summary>
        /// Adds a new connection ID for a user to the presence tracking.
        /// Also sets an expiration on the connection key.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connectionId">The SignalR connection ID.</param>
        public async Task AddConnectionAsync(Guid userId, string connectionId)
        {
            try
            {
                var userKey = $"presence:user:{userId}";
                var connKey = $"presence:conn:{connectionId}";

                var batch = _redis.CreateBatch();

                // Add the connection ID to the set of connections for the user
                var addToSetTask = batch.SetAddAsync(userKey, connectionId);

                // Store the user ID associated with the connection ID
                // Set an expiration time for this key as a safety net
                var setUserTask = batch.StringSetAsync(connKey, userId.ToString(), PresenceKeyExpiration);

                // Execute the batch of commands
                batch.Execute();

                // Wait for the batch operations to complete
                await Task.WhenAll(addToSetTask, setUserTask);

                _logger.LogInformation("Added connection {ConnectionId} for user {UserId}", connectionId, userId);

                // Mark the user as online in persistent storage
                await _userService.SetUserOnlineStatusAsync(userId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding connection {ConnectionId} for user {UserId}", connectionId, userId);
            }
        }

        /// <summary>
        /// Removes a connection ID from the presence tracking.
        /// If this was the last connection for the user, marks the user as offline.
        /// </summary>
        /// <param name="connectionId">The SignalR connection ID.</param>
        public async Task RemoveConnectionAsync(string connectionId)
        {
            try
            {
                var connKey = $"presence:conn:{connectionId}";
                // Get the user ID associated with the connection ID
                var userIdString = await _redis.StringGetAsync(connKey);

                if (Guid.TryParse(userIdString, out var userId))
                {
                    var userKey = $"presence:user:{userId}";

                    var batch = _redis.CreateBatch();

                    // Remove the connection ID from the user's set of connections
                    var setRemove = batch.SetRemoveAsync(userKey, connectionId);
                    // Delete the connection-to-user mapping key
                    var keyDelete = batch.KeyDeleteAsync(connKey);

                    // Execute the batch of commands
                    batch.Execute();

                    // Wait for the batch operations to complete
                    await Task.WhenAll(setRemove, keyDelete);

                    _logger.LogInformation("Removed connection {ConnectionId} for user {UserId}", connectionId, userId);

                    // Check if the user has any remaining connections
                    var remaining = await _redis.SetLengthAsync(userKey);
                    if (remaining == 0)
                    {
                        _logger.LogInformation("User {UserId} has no remaining connections. Marking as offline.", userId);
                        // If no connections remain, mark the user as offline in persistent storage
                        await _userService.SetUserOnlineStatusAsync(userId, false);
                    }
                }
                else
                {
                    _logger.LogWarning("Attempted to remove connection {ConnectionId} but could not find associated user ID.", connectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing connection {ConnectionId}", connectionId);
            }
        }

        /// <summary>
        /// Gets all active connection IDs for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An enumerable of connection IDs.</returns>
        public async Task<IEnumerable<string>> GetConnectionsForUserAsync(Guid userId)
        {
            try
            {
                var userKey = $"presence:user:{userId}";
                // Retrieve all members from the user's set of connections
                var connections = await _redis.SetMembersAsync(userKey);
                // Convert RedisValue array to string enumerable
                return connections.Select(c => c.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections for user {UserId}", userId);
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the user ID associated with a given connection ID.
        /// </summary>
        /// <param name="connectionId">The SignalR connection ID.</param>
        /// <returns>The user ID, or Guid.Empty if not found or invalid.</returns>
        public async Task<Guid> GetUserIdByConnectionIdAsync(string connectionId)
        {
            try
            {
                var connKey = $"presence:conn:{connectionId}";
                // Retrieve the user ID from the connection-to-user mapping key
                var value = await _redis.StringGetAsync(connKey);
                // Safely parse the string value to a Guid
                return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID for connection {ConnectionId}", connectionId);
                return Guid.Empty;
            }
        }
    }
}
