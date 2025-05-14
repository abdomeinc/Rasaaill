using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Client.Core.Comms.Services
{
    public class SignalRClientService : Interfaces.ISignalRClientService, IDisposable, IAsyncDisposable
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SignalRClientService> _logger;
        private HubConnection _connection;
        private readonly Interfaces.IClientMessageHandler _handler;

        public SignalRClientService(ILogger<SignalRClientService> logger, Interfaces.IClientMessageHandler handler, IConfiguration config)
        {
            _logger = logger;
            _handler = handler;
            _config = config;
            _connection = null!;
        }

        public void Initialize(string jwtToken)
        {
            var serverUrl = _config["SignalR:ServerUrl"];
            if (string.IsNullOrEmpty(serverUrl)) // Better check for null or empty
            {
                _logger.LogError("SignalR:ServerUrl configuration is missing or empty.");
                throw new InvalidOperationException("SignalR server URL is not configured."); // More specific exception
            }

            // Build the connection here, replacing the one from the constructor
            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl, options =>
                {
                    options.AccessTokenProvider = async () => await Task.FromResult(jwtToken); // async lambda is good
                })
                .WithAutomaticReconnect(
                [
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                ])
                .Build();

            RegisterCallbacks(); // Use the callbacks from the HubConnectionBuilder events if desired, or keep your existing ones.
        }

        // Remove ReconnectWithDelay method entirely.
        // Update the Closed handler to just log/notify UI if needed, as WithAutomaticReconnect handles retries.

        private void RegisterCallbacks()
        {
            // Use the OnReconnecting/OnReconnected/OnClosed from the HubConnectionBuilder as shown above
            // OR keep your existing ones, but remove the call to ReconnectWithDelay in Closed.

            _connection.Reconnecting += (error =>
            {
                ShowConnectionStatus("Reconnecting...");
                _logger.LogInformation("Connection is reconnecting...");
                return Task.CompletedTask;
            });

            _connection.Reconnected += connectionId =>
            {
                ShowConnectionStatus("Reconnected"); // You might want to show "Reconnected" or similar
                _logger.LogInformation("Reconnected with ID: {ConnectionId}", connectionId);
                SyncAfterReconnect(); // This is correct
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                ShowConnectionStatus("Disconnected"); // This indicates the connection is closed, possibly after retries failed
                _logger.LogInformation("Connection closed. Error: {Error}", error?.Message);
                // Remove the call to _ = ReconnectWithDelay();
                return Task.CompletedTask;
            };

            // Message/Event handlers remain the same
            _connection.On<Entities.Dtos.MessageDto>("ReceiveMessage", _handler.OnMessageReceived);
            _connection.On<Guid>("Typing", _handler.OnTyping);
            _connection.On<Guid>("MessageSeen", _handler.OnMessageSeen);
            _connection.On<List<Guid>>("UpdateTypingStatus", _handler.OnUpdateTypingStatus);
            // Make sure these match the server-side method names and parameter types exactly
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
                // Fetch the typing status for the relevant conversation(s)
                _connection.InvokeAsync("GetTypingStatusForAllUsers", Guid.Empty/*currentConversationId*/);

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

        private async Task<List<Entities.Dtos.MessageDto>> GetUnreadMessagesForUserAsync()
        {
            // Get the unread messages for the current user from the server
            var unreadMessages = await _connection.InvokeAsync<List<Entities.Dtos.MessageDto>>("GetUnreadMessages", Guid.Empty/*currentUserId*/);
            return unreadMessages;
        }

        public async Task ConnectAsync() => await _connection.StartAsync();
        public async Task DisconnectAsync() => await _connection.StopAsync();

        public async Task SendMessageAsync(Entities.Dtos.MessageDto message) =>
            await _connection.InvokeAsync("SendMessageAsync", message);

        public async Task JoinConversationAsync(Guid conversationId) =>
            await _connection.InvokeAsync("JoinConversationAsync", conversationId);


        private bool disposedValue;

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    if (_connection != null)
                    {
                        await _connection.DisposeAsync();
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                _connection = null!; // Null out the connection

                disposedValue = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            // Do not change this code. Put cleanup code in 'DisposeAsync(bool disposing)' method
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects) synchronosly if possible
                    // For HubConnection, prefer DisposeAsync
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
