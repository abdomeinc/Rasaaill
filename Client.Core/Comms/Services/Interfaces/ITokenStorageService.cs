namespace Client.Core.Comms.Services.Interfaces
{
    public interface ITokenStorageService
    {
        void Save(string jwt);
        string? Load();
        void Clear();
    }
}
