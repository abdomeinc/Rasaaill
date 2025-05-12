namespace Entities.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public Shared.Enums.ConversationType ConversationType { get; set; }

        public Shared.Enums.NotificationType NotificationType { get; set; }

        public int NewMessagesCount { get; set; }

        public required Message LastMessage { get; set; }

        public DateTime CreationDate { get; set; }

        //public virtual List<User> Participants { get; set; } = [];

        public virtual List<Message> Messages { get; set; } = []; // will handle latest 50 messages for performance optimize

        public string? GroupName { get; set; } // for conversation type Group

        public string? GroupIconUrl { get; set; } // for conversation type Group
    }
}
