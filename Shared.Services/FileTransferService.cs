using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace Shared.Services
{
    public class FileTransferService : Interfaces.IFileTransferService
    {
        private const int TransferPort = 52222;
        private readonly TcpListener _listener;
        private readonly Aes _aes;

        public FileTransferService()
        {
            _listener = new TcpListener(IPAddress.Any, TransferPort);
            _aes = Aes.Create();
            _aes.GenerateKey();
            _aes.GenerateIV();
        }

        public async Task SendFileAsync(string filePath, string peerId)
        {
            var peerIp = await ResolvePeerIp(peerId);
            using var client = new TcpClient();

            await client.ConnectAsync(peerIp, TransferPort);
            using var stream = client.GetStream();

            // Send encryption parameters
            await stream.WriteAsync(_aes.IV, 0, _aes.IV.Length);

            using var cryptoStream = new CryptoStream(stream,
                _aes.CreateEncryptor(), CryptoStreamMode.Write);

            using var fileStream = System.IO.File.OpenRead(filePath);
            await fileStream.CopyToAsync(cryptoStream);
        }

        private async Task<string> ResolvePeerIp(string peerId)
        {
            // Implement STUN client resolution
            var stunClient = new NatTraversalService();
            var response = await stunClient.GetPublicEndPointAsync("stun.l.google.com", 19302);
            return response/*.PublicEndPoint*/.Address.ToString();
        }

        public void StartListening()
        {
            _listener.Start();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    ProcessIncomingFile(client);
                }
            });
        }

        private void ProcessIncomingFile(TcpClient client)
        {
            // Handle incoming file transfer
        }
    }

    public class NatTraversalService
    {
        public async Task<IPEndPoint> GetPublicEndPointAsync(string stunServer = "stun.l.google.com", int port = 19302)
        {
            using var udpClient = new UdpClient();
            var stunIp = Dns.GetHostAddresses(stunServer).FirstOrDefault();

            // Simple STUN binding request
            var request = new byte[] { 0x00, 0x01, 0x00, 0x00 };
            await udpClient.SendAsync(request, request.Length, new IPEndPoint(stunIp, port));

            var response = await udpClient.ReceiveAsync();
            return response.RemoteEndPoint;
        }
    }
}
