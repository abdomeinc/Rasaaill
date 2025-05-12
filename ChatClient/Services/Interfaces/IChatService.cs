namespace ChatClient.Services.Interfaces
{
    public interface IChatService
    {
        Task ConnectAsync(string serverUrl);
        Task SendMessageAsync(Shared.Models.Message message);
        event EventHandler<Shared.Models.Message>? MessageReceived;
    }
}
