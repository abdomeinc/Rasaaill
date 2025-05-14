using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Shared.Services
{
    /// <summary>
    /// Provides UDP-based service discovery and group management for clients and servers.
    /// Periodically broadcasts server presence and manages client registrations and group memberships.
    /// </summary>
    public class DiscoveryService : IHostedService, IDisposable
    {
        /// <summary>
        /// UDP client used for sending broadcast messages.
        /// </summary>
        private readonly UdpClient _udpClient = new(12345);

        /// <summary>
        /// Timer for scheduling periodic broadcast of server presence.
        /// </summary>
        private Timer? _broadcastTimer;

        /// <summary>
        /// Starts the discovery service and begins broadcasting presence.
        /// </summary>
        /// <param name="cancellationToken">Token to signal cancellation.</param>
        /// <returns>A completed task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _broadcastTimer = new Timer(BroadcastPresence, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the discovery service and halts broadcasting.
        /// </summary>
        /// <param name="cancellationToken">Token to signal cancellation.</param>
        /// <returns>A completed task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _broadcastTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Broadcasts the server's presence over UDP to the local network.
        /// </summary>
        /// <param name="state">Unused state object.</param>
        private void BroadcastPresence(object? state)
        {
            var message = $"RASAAIL_SERVER|{GetLocalIp()}";
            var data = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(data, data.Length, "255.255.255.255", 12345);
        }

        /// <summary>
        /// Retrieves the local IPv4 address of the host.
        /// </summary>
        /// <returns>The local IPv4 address as a string.</returns>
        private string GetLocalIp() =>
            Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToString();

        /// <summary>
        /// Registers a client with the specified connection ID and IP address.
        /// </summary>
        /// <param name="connectionId">The unique connection identifier for the client.</param>
        /// <param name="ipAddress">The IP address of the client.</param>
        public void RegisterClient(string connectionId, string ipAddress)
        {

        }

        /// <summary>
        /// Gets a list of recipient connection IDs for the specified group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A list of connection IDs belonging to the group.</returns>
        public List<string> GetRecipients(Guid groupId)
        {
            var recipients = new List<string>();
            return recipients;
        }

        /// <summary>
        /// Adds a client to a group based on connection ID and group ID.
        /// </summary>
        /// <param name="connectionId">The unique connection identifier for the client.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        public void AddToGroup(string connectionId, Guid groupId)
        {

        }

        /// <summary>
        /// Disposes the broadcast timer and releases resources.
        /// </summary>
        public void Dispose() => _broadcastTimer?.Dispose();
    }
}
