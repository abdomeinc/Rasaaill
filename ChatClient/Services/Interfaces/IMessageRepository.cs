namespace ChatClient.Services.Interfaces
{
    public interface IMessageRepository
    {
        void StoreMessage(Shared.Models.Message message);
        IEnumerable<Shared.Models.Message> GetPendingMessages();
    }
}
