using Microsoft.AspNetCore.SignalR.Client;

namespace ChatClient.Services.Network
{
    public class ChatService : Interfaces.IChatService
    {
        private HubConnection _hubConnection;
        private readonly Interfaces.IMessageRepository _repository;

        public ChatService(Interfaces.IMessageRepository repository, HubConnection hubConnection)
        {
            _repository = repository;
            _hubConnection = hubConnection;
        }

        public async Task ConnectAsync(string serverUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<Shared.Models.Message>("ReceiveMessage", message =>
            {
                _repository.StoreMessage(message);
                MessageReceived?.Invoke(this, message);
            });

            await _hubConnection.StartAsync();
        }

        public async Task SendMessageAsync(Shared.Models.Message message)
        {
            if (_hubConnection.State != HubConnectionState.Connected)
                throw new InvalidOperationException("Not connected");

            await _hubConnection.SendAsync("SendMessage", message);
            message.Status = Shared.Enums.MessageStatus.Pending;
            _repository.StoreMessage(message/* with { Status = Shared.Enums.MessageStatus.Pending }*/);
        }

        public event EventHandler<Shared.Models.Message>? MessageReceived;

    }
}
