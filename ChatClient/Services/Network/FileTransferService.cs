using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace ChatClient.Services.Network
{
    public class FileTransferService: Interfaces.IFileTransferService
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
            using var stunClient = new StunClient("stun.l.google.com", 19302);
            var response = await stunClient.QueryAsync();
            return response.PublicEndPoint.Address.ToString();
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
}
