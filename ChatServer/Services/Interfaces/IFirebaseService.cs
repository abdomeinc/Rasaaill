namespace ChatServer.Services.Interfaces
{
    public interface IFirebaseService
    {
        Task StoreMessageAsync(Shared.Models.Message message);
    }
}
