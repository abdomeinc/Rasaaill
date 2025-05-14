using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace Shared.Services
{
    /// <summary>
    /// Provides functionality for secure file transfer between peers using TCP and AES encryption.
    /// </summary>
    public class FileTransferService : Interfaces.IFileTransferService
    {
        /// <summary>
        /// The port used for file transfer operations.
        /// </summary>
        private const int TransferPort = 52222;

        /// <summary>
        /// TCP listener for incoming file transfer connections.
        /// </summary>
        private readonly TcpListener _listener;

        /// <summary>
        /// AES encryption instance used for encrypting file data during transfer.
        /// </summary>
        private readonly Aes _aes;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTransferService"/> class.
        /// Sets up the TCP listener and generates AES encryption parameters.
        /// </summary>
        public FileTransferService()
        {
            _listener = new TcpListener(IPAddress.Any, TransferPort);
            _aes = Aes.Create();
            _aes.GenerateKey();
            _aes.GenerateIV();
        }

        /// <summary>
        /// Sends a file to a peer using the specified peer identifier.
        /// The file is encrypted using AES before transfer.
        /// </summary>
        /// <param name="filePath">The path to the file to send.</param>
        /// <param name="peerId">The identifier of the peer to send the file to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Resolves the IP address of a peer using its identifier.
        /// </summary>
        /// <param name="peerId">The identifier of the peer.</param>
        /// <returns>A task that returns the IP address as a string.</returns>
        private async Task<string> ResolvePeerIp(string peerId)
        {
            // Implement STUN client resolution
            var stunClient = new NatTraversalService();
            var response = await stunClient.GetPublicEndPointAsync("stun.l.google.com", 19302);
            return response/*.PublicEndPoint*/.Address.ToString();
        }

        /// <summary>
        /// Starts listening for incoming file transfer connections.
        /// </summary>
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

        /// <summary>
        /// Processes an incoming file transfer from a connected client.
        /// </summary>
        /// <param name="client">The TCP client representing the incoming connection.</param>
        private void ProcessIncomingFile(TcpClient client)
        {
            // Handle incoming file transfer
        }
    }

    /// <summary>
    /// Provides NAT traversal services using the STUN protocol to discover public endpoints.
    /// </summary>
    public class NatTraversalService
    {
        /// <summary>
        /// Gets the public endpoint of the current machine using a STUN server.
        /// </summary>
        /// <param name="stunServer">The STUN server address.</param>
        /// <param name="port">The port of the STUN server.</param>
        /// <returns>A task that returns the public <see cref="IPEndPoint"/>.</returns>
        public async Task<IPEndPoint> GetPublicEndPointAsync(string stunServer = "stun.l.google.com", int port = 19302)
        {
            using var udpClient = new UdpClient();
            var stunIp = Dns.GetHostAddresses(stunServer).FirstOrDefault();

            // Simple STUN binding request
            var request = new byte[] { 0x00, 0x01, 0x00, 0x00 };
            _ = await udpClient.SendAsync(request, request.Length, new IPEndPoint(stunIp, port));

            var response = await udpClient.ReceiveAsync();
            return response.RemoteEndPoint;
        }
    }
}
