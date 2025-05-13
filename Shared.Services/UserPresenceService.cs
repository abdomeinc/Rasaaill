using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Shared.Services
{
    public class UserPresenceService : Interfaces.IUserPresenceService
    {
        private readonly IDatabase _redis;

        private readonly ILogger<UserPresenceService> _logger;
        private readonly ConcurrentDictionary<string, Guid> _connectionToUserMap = new();
        private readonly ConcurrentDictionary<Guid, List<string>> _userToConnections = new();
        private readonly Interfaces.IConversationService _conversationService;

        public UserPresenceService(ILogger<UserPresenceService> logger, Interfaces.IConversationService conversationService, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _conversationService = conversationService;
            _redis = redis.GetDatabase();
        }

        public async Task AddConnectionAsync(Guid userId, string connectionId)
        {
            try
            {
                var userKey = $"presence:user:{userId}";
                var connKey = $"presence:conn:{connectionId}";

                var batch = _redis.CreateBatch();
                var addToSetTask = batch.SetAddAsync(userKey, connectionId);
                var setUserTask = batch.StringSetAsync(connKey, userId.ToString());

                // Must call Execute and await individual tasks
                batch.Execute();
                await Task.WhenAll(addToSetTask, setUserTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding connection for user {UserId}", userId);
            }
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            try
            {
                var connKey = $"presence:conn:{connectionId}";
                var userIdString = await _redis.StringGetAsync(connKey);

                if (Guid.TryParse(userIdString, out var userId))
                {
                    var userKey = $"presence:user:{userId}";

                    var batch = _redis.CreateBatch();
                    var setRemove = batch.SetRemoveAsync(userKey, connectionId);
                    var keyDelete= batch.KeyDeleteAsync(connKey);

                    // Must call Execute and await individual tasks
                    batch.Execute();
                    await Task.WhenAll(setRemove, keyDelete);

                    // Optional: If user has no more connections, you can mark them as offline
                    var remaining = await _redis.SetLengthAsync(userKey);
                    if (remaining == 0)
                    {
                        // Trigger user-offline logic if needed
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing connection {ConnectionId}", connectionId);
            }
        }

        public async Task<IEnumerable<string>> GetConnectionsForUserAsync(Guid userId)
        {
            try
            {
                var userKey = $"presence:user:{userId}";
                var connections = await _redis.SetMembersAsync(userKey);
                return connections.Select(c => c.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections for user {UserId}", userId);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<Guid> GetUserIdByConnectionIdAsync(string connectionId)
        {
            try
            {
                var connKey = $"presence:conn:{connectionId}";
                var value = await _redis.StringGetAsync(connKey);
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
