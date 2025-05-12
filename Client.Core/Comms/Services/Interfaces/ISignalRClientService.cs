namespace Client.Core.Comms.Services.Interfaces
{
    public interface ISignalRClientService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendMessageAsync(Entities.Dtos.MessageDto message);
        Task JoinConversationAsync(Guid conversationId);
    }
}
