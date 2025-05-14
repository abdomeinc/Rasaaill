namespace Client.Core.Comms.Services.Interfaces
{
    public interface ITokenStorageService
    {
        void Save(string jwt);
        string? Load();
        void Clear();


        Task SaveTokenAsync(string accessToken, string refreshToken);
        Task SaveTokenAsync(string accessToken);
        Task<(string AccessToken, string RefreshToken)> LoadTokenAndRefreshAsync();
        Task<string> LoadTokenAsync();
        Task DeleteTokenAsync();
    }
}
