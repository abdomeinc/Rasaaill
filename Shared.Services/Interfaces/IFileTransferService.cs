namespace Shared.Services.Interfaces
{
    /// <summary>
    /// Provides methods for sending files and listening for incoming file transfers between peers.
    /// </summary>
    public interface IFileTransferService
    {
        /// <summary>
        /// Asynchronously sends a file to the specified peer.
        /// </summary>
        /// <param name="filePath">The full path of the file to send.</param>
        /// <param name="peerId">The identifier of the peer to send the file to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendFileAsync(string filePath, string peerId);

        /// <summary>
        /// Starts listening for incoming file transfer requests.
        /// </summary>
        void StartListening();
    }
}
