namespace Shared.Services.Interfaces
{
    public interface IUserService
    {
        Task SetUserOnlineStatusAsync(Guid userId, bool isOnline);
    }
}
