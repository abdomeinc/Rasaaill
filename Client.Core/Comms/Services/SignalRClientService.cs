using Entities.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Client.Core.Comms.Services
{
    public class SignalRClientService : Interfaces.ISignalRClientService
    {
        private readonly HubConnection _connection;
        private readonly Interfaces.IClientMessageHandler _handler;

        public SignalRClientService(Interfaces.IClientMessageHandler handler, IConfiguration config)
        {
            _handler = handler;
            _connection = new HubConnectionBuilder()
                .WithUrl(config["SignalR:ServerUrl"] ?? "")
                .WithAutomaticReconnect()
                .Build();

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _connection.On<MessageDto>("ReceiveMessage", _handler.OnMessageReceived);
            _connection.On<Guid>("Typing", _handler.OnTyping);
            _connection.On<Guid>("MessageSeen", _handler.OnMessageSeen);
        }

        public async Task ConnectAsync() => await _connection.StartAsync();
        public async Task DisconnectAsync() => await _connection.StopAsync();

        public async Task SendMessageAsync(MessageDto message) =>
            await _connection.InvokeAsync("SendMessageAsync", message);

        public async Task JoinConversationAsync(Guid conversationId) =>
            await _connection.InvokeAsync("JoinConversationAsync", conversationId);

    }
}
