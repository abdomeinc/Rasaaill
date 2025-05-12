using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ChatServer.Services
{
    public class DiscoveryService : Interfaces.IDiscoveryService
    {
        private readonly UdpClient _udpClient = new(12345);
        private Timer? _broadcastTimer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _broadcastTimer = new Timer(BroadcastPresence, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void BroadcastPresence(object? state)
        {
            var message = $"RASAAIL_SERVER|{GetLocalIp()}";
            var data = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(data, data.Length, "255.255.255.255", 12345);
        }

        private string GetLocalIp() =>
            Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToString();

        public void RegisterClient(string connectionId, string ipAddress)
        {

        }

        public List<string> GetRecipients(Guid groupId)
        {
            var recipients = new List<string>();
            return recipients;
        }

        public void AddToGroup(string connectionId, Guid groupId)
        {

        }
    }
}
