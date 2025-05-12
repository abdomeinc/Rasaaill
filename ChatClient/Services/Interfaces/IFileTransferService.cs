namespace ChatClient.Services.Interfaces
{
    public interface IFileTransferService
    {
        Task SendFileAsync(string filePath, string peerId);
        void StartListening();
    }
}
