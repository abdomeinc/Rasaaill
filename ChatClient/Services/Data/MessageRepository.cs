using LiteDB;

namespace ChatClient.Services.Data
{
    public class MessageRepository : Interfaces.IMessageRepository
    {
        private readonly LiteDatabase _db;

        public MessageRepository()
        {
            _db = new LiteDatabase("Filename=messages.db;Connection=shared");
            var messages = _db.GetCollection<Shared.Models.Message>();
            messages.EnsureIndex(x => x.Timestamp);
            messages.EnsureIndex(x => x.Status);
        }

        public void StoreMessage(Shared.Models.Message message)
        {
            var existing = _db.GetCollection<Shared.Models.Message>()
                .FindOne(x => x.Id == message.Id);

            if (existing == null)
                _db.GetCollection<Shared.Models.Message>().Insert(message);
            else
                _db.GetCollection<Shared.Models.Message>().Update(message);
        }

        public IEnumerable<Shared.Models.Message> GetPendingMessages()
        {
            return _db.GetCollection<Shared.Models.Message>()
                .Find(x => x.Status == Shared.Enums.MessageStatus.Pending)
                .OrderBy(x => x.Timestamp);
        }
    }
}
