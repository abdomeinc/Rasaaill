namespace Shared.Services.Interfaces
{
    public interface IUserPresenceService
    {
        Task AddConnectionAsync(Guid userId, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
        Task<IEnumerable<string>> GetConnectionsForUserAsync(Guid userId);
        Task<Guid> GetUserIdByConnectionIdAsync(string connectionId);
    }
}
