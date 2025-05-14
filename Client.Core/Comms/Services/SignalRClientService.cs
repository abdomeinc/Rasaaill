using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Client.Core.Comms.Services
{
    /// <summary>
    /// Provides a client-side service for managing SignalR connections, message handling, and reconnection logic.
    /// </summary>
    public class SignalRClientService : Interfaces.ISignalRClientService, IDisposable, IAsyncDisposable
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SignalRClientService> _logger;
        private HubConnection _connection;
        private readonly Interfaces.IClientMessageHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRClientService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="handler">The message handler for incoming SignalR messages.</param>
        /// <param name="config">The application configuration.</param>
        public SignalRClientService(ILogger<SignalRClientService> logger, Interfaces.IClientMessageHandler handler, IConfiguration config)
        {
            _logger = logger;
            _handler = handler;
            _config = config;
            _connection = null!;
        }

        /// <summary>
        /// Initializes the SignalR connection with the specified JWT token.
        /// </summary>
        /// <param name="jwtToken">The JWT token for authentication.</param>
        /// <exception cref="InvalidOperationException">Thrown if the server URL is not configured.</exception>
        public void Initialize(string jwtToken)
        {
            var serverUrl = _config["SignalR:ServerUrl"];
            if (string.IsNullOrEmpty(serverUrl))
            {
                _logger.LogError("SignalR:ServerUrl configuration is missing or empty.");
                throw new InvalidOperationException("SignalR server URL is not configured.");
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl, options =>
                {
                    options.AccessTokenProvider = async () => await Task.FromResult(jwtToken);
                })
                .WithAutomaticReconnect(
                [
                    TimeSpan.Zero,
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                ])
                .Build();

            RegisterCallbacks();
        }

        /// <summary>
        /// Registers SignalR event handlers and message callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            _connection.Reconnecting += (error =>
            {
                ShowConnectionStatus("Reconnecting...");
                _logger.LogInformation("Connection is reconnecting...");
                return Task.CompletedTask;
            });

            _connection.Reconnected += connectionId =>
            {
                ShowConnectionStatus("Reconnected");
                _logger.LogInformation("Reconnected with ID: {ConnectionId}", connectionId);
                SyncAfterReconnect();
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                ShowConnectionStatus("Disconnected");
                _logger.LogInformation("Connection closed. Error: {Error}", error?.Message);
                return Task.CompletedTask;
            };

            _connection.On<Entities.Dtos.MessageDto>("ReceiveMessage", _handler.OnMessageReceived);
            _connection.On<Guid>("Typing", _handler.OnTyping);
            _connection.On<Guid>("MessageSeen", _handler.OnMessageSeen);
            _connection.On<List<Guid>>("UpdateTypingStatus", _handler.OnUpdateTypingStatus);
        }

        /// <summary>
        /// (Obsolete) Attempts to reconnect with a delay if the connection is lost.
        /// </summary>
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

        /// <summary>
        /// Synchronizes client state after a successful reconnection.
        /// </summary>
        private void SyncAfterReconnect()
        {
            _ = _connection.InvokeAsync("SetUserOnline", Guid.Empty/*CurrentUserId*/);
            LoadUnreadMessages();
            ReInitializeTypingIndicators();
        }

        /// <summary>
        /// Displays the current connection status to the user interface.
        /// </summary>
        /// <param name="v">The status message to display.</param>
        private void ShowConnectionStatus(string v)
        {

        }

        /// <summary>
        /// Re-initializes typing indicators after reconnection.
        /// </summary>
        private void ReInitializeTypingIndicators()
        {
            try
            {
                _connection.InvokeAsync("GetTypingStatusForAllUsers", Guid.Empty/*currentConversationId*/);
                _logger.LogInformation("Re-initialized typing indicators.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error re-initializing typing indicators: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads unread messages for the current user and notifies the UI.
        /// </summary>
        private async void LoadUnreadMessages()
        {
            try
            {
                var unreadMessages = await GetUnreadMessagesForUserAsync();

                if (unreadMessages != null && unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        _logger.LogInformation($"Unread message: {message.Content}");
                        await _connection.InvokeAsync("NotifyUnreadMessages", unreadMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading unread messages: " + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves unread messages for the current user from the server.
        /// </summary>
        /// <returns>A list of unread message DTOs.</returns>
        private async Task<List<Entities.Dtos.MessageDto>> GetUnreadMessagesForUserAsync()
        {
            var unreadMessages = await _connection.InvokeAsync<List<Entities.Dtos.MessageDto>>("GetUnreadMessages", Guid.Empty/*currentUserId*/);
            return unreadMessages;
        }

        /// <summary>
        /// Starts the SignalR connection asynchronously.
        /// </summary>
        public async Task ConnectAsync() => await _connection.StartAsync();

        /// <summary>
        /// Stops the SignalR connection asynchronously.
        /// </summary>
        public async Task DisconnectAsync() => await _connection.StopAsync();

        /// <summary>
        /// Sends a message to the server asynchronously.
        /// </summary>
        /// <param name="message">The message DTO to send.</param>
        public async Task SendMessageAsync(Entities.Dtos.MessageDto message) =>
            await _connection.InvokeAsync("SendMessageAsync", message);

        /// <summary>
        /// Joins a conversation asynchronously.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to join.</param>
        public async Task JoinConversationAsync(Guid conversationId) =>
            await _connection.InvokeAsync("JoinConversationAsync", conversationId);

        private bool disposedValue;

        /// <summary>
        /// Disposes the SignalR connection and managed resources asynchronously.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        await _connection.DisposeAsync();
                    }
                }
                _connection = null!;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the SignalR connection and managed resources asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the SignalR connection and managed resources synchronously.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // For HubConnection, prefer DisposeAsync
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the SignalR connection and managed resources synchronously.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
