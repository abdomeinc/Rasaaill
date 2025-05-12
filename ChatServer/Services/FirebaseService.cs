using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace ChatServer.Services
{
    public class FirebaseService : Interfaces.IFirebaseService
    {
        private readonly FirestoreDb _db;

        public FirebaseService(IConfiguration config)
        {
            var cred = GoogleCredential.FromJson(config["Firebase:Credentials"]);
            FirebaseApp.Create(new AppOptions { Credential = cred });
            _db = FirestoreDb.Create(config["Firebase:ProjectId"]);
        }

        public async Task StoreMessageAsync(Shared.Models.Message message)
        {
            var docRef = _db.Collection("messages").Document(message.Id);
            await docRef.SetAsync(message);
        }

        //public async Task CreateGroupAsync(Shared.Models.ChatGroup group)
        //{
        //    await _db.Collection("groups").Document(group.Id).SetAsync(group);
        //}
    }
}
