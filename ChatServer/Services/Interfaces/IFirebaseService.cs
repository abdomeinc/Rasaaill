namespace ChatServer.Services.Interfaces
{
    public interface IFirebaseService
    {
        Task StoreMessageAsync(Shared.Models.Message message);
        Task<FirebaseAdmin.Auth.FirebaseToken> VerifyToken(string idToken);
    }
}
