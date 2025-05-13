using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Client.Core.Comms.Services
{
    public class SignalRClientService : Interfaces.ISignalRClientService
    {
        private readonly ILogger<SignalRClientService> _logger;
        private readonly HubConnection _connection;
        private readonly Interfaces.IClientMessageHandler _handler;

        public SignalRClientService(ILogger<SignalRClientService> logger, Interfaces.IClientMessageHandler handler, IConfiguration config)
        {
            _logger = logger;
            _handler = handler;
            _connection = new HubConnectionBuilder()
                .WithUrl(config["SignalR:ServerUrl"] ?? "")
                .WithAutomaticReconnect([
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                    ])
                .Build();

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            _connection.Reconnecting += error =>
            {
                ShowConnectionStatus("Reconnecting...");
                return Task.CompletedTask;
            };

            _connection.Reconnected += connectionId =>
            {
                ShowConnectionStatus("Connected");
                SyncAfterReconnect(); // Re-init presence, messages, etc.
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                ShowConnectionStatus("Disconnected");
                // Optionally auto-retry
                _ = ReconnectWithDelay();
                return Task.CompletedTask;
            };


            _connection.On<Entities.Dtos.MessageDto>("ReceiveMessage", _handler.OnMessageReceived);
            _connection.On<Guid>("Typing", _handler.OnTyping);
            _connection.On<Guid>("MessageSeen", _handler.OnMessageSeen);
        }

        private async Task ReconnectWithDelay()
        {
            var delay = TimeSpan.FromSeconds(5);
            while (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _connection.StartAsync();
                    _logger.LogInformation("Reconnected manually.");
                }
                catch
                {
                    _logger.LogError("Reconnect failed, retrying...");
                    await Task.Delay(delay);
                }
            }
        }

        private void SyncAfterReconnect()
        {
            // Re-send "online" status
            _ = _connection.InvokeAsync("SetUserOnline", Guid.Empty/*CurrentUserId*/);

            // Optionally reload messages or notify UI
            LoadUnreadMessages();
            ReInitializeTypingIndicators();
        }

        private void ShowConnectionStatus(string v)
        {

        }

        private void ReInitializeTypingIndicators()
        {
            try
            {
                // Check the current typing state for all users in active conversations.
                // If necessary, clear out the typing state or notify users accordingly.

                // This could be a signal to refresh typing status, for example:
                _connection.InvokeAsync("GetTypingStatusForAllUsers"); // Assuming such a method exists on the server side.

                _logger.LogInformation("Re-initialized typing indicators.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error re-initializing typing indicators: " + ex.Message);
            }
        }
        private async void LoadUnreadMessages()
        {
            try
            {
                // Assuming there's a method to get unread messages for a user from a service.
                var unreadMessages = await GetUnreadMessagesForUserAsync();

                if (unreadMessages != null && unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        // Notify the user of the unread messages, update UI, etc.
                        _logger.LogInformation($"Unread message: {message.Content}");

                        // Optionally, invoke a SignalR method to notify the UI about the unread messages.
                        await _connection.InvokeAsync("NotifyUnreadMessages", unreadMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading unread messages: " + ex.Message);
            }
        }

        private Task<List<Entities.Dtos.MessageDto>> GetUnreadMessagesForUserAsync()
        {
            // Simulate fetching unread messages from a server, database, or cache.
            return Task.FromResult(new List<Entities.Dtos.MessageDto>
            {
                new Entities.Dtos.MessageDto { Content = "Hello there!", SenderId = Guid.NewGuid(), Timestamp = DateTime.UtcNow },
                new Entities.Dtos.MessageDto { Content = "How are you?", SenderId = Guid.NewGuid(), Timestamp = DateTime.UtcNow }
            });
        }

        public async Task ConnectAsync() => await _connection.StartAsync();
        public async Task DisconnectAsync() => await _connection.StopAsync();

        public async Task SendMessageAsync(Entities.Dtos.MessageDto message) =>
            await _connection.InvokeAsync("SendMessageAsync", message);

        public async Task JoinConversationAsync(Guid conversationId) =>
            await _connection.InvokeAsync("JoinConversationAsync", conversationId);

    }
}
