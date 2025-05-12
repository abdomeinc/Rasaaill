using Shared.Services.Interfaces;
using System.Collections.Concurrent;

namespace Shared.Services
{
    public class UserPresenceService : Interfaces.IUserPresenceService
    {
        private readonly ConcurrentDictionary<string, Guid> _connectionToUserMap = new();
        private readonly ConcurrentDictionary<Guid, List<string>> _userToConnections = new();
        private readonly Interfaces.IConversationService _conversationService;

        public UserPresenceService(Interfaces.IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public void AddConnection(Guid userId, string connectionId)
        {
            _connectionToUserMap[connectionId] = userId;
            _userToConnections.AddOrUpdate(userId,
                _ => new List<string> { connectionId },
                (_, list) => { list.Add(connectionId); return list; });
        }

        public void RemoveConnection(string connectionId)
        {
            if (_connectionToUserMap.TryRemove(connectionId, out var userId))
            {
                if (_userToConnections.TryGetValue(userId, out var conns))
                {
                    conns.Remove(connectionId);
                    if (conns.Count == 0)
                        _userToConnections.TryRemove(userId, out _);
                }
            }
        }

        public IEnumerable<string> GetConnectionsForUser(Guid userId) =>
            _userToConnections.TryGetValue(userId, out var conns) ? conns : [];

        public Guid GetUserIdByConnectionId(string connectionId) =>
            _connectionToUserMap.TryGetValue(connectionId, out var uid) ? uid : Guid.Empty;
    }
}
