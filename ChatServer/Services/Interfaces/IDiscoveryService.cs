namespace ChatServer.Services.Interfaces
{
    public interface IDiscoveryService
    {
        Task StartAsync(CancellationToken cancellationToken);
        void RegisterClient(string connectionId, string ipAddress);
        List<string> GetRecipients(Guid groupId);
        void AddToGroup(string connectionId, Guid groupId);
    }
}
