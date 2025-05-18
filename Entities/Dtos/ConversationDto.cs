using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Entities.Dtos
{
    public class ConversationDto : INotifyPropertyChanged
    {
        public Guid Id { get; set; }

        public Shared.ConversationType ConversationType { get; set; }

        public Shared.NotificationType NotificationType { get; set; }

        private MessageDto? lastMessage { get; set; } = default!;
        public MessageDto? LastMessage { get => lastMessage; set { lastMessage = value; OnPropertyChanged(); } }

        public DateTime CreationDate { get; set; }

        public virtual ObservableCollection<MessageDto> Messages { get; set; } = []; // will handle latest 50 messages for performance optimize

        public string? DisplayName { get; set; } // for conversation type Group

        public string? ImageUrl { get; set; } // for conversation type Group

        private bool isSelected { get; set; }
        public bool IsSelected { get => isSelected; set { isSelected = value; OnPropertyChanged(); } }

        private int unreadCount { get; set; }
        public int UnreadCount { get => unreadCount; set { unreadCount = value; OnPropertyChanged(); } }

        private bool isFavorite { get; set; }
        public bool IsFavorite { get => isFavorite; set { isFavorite = value; OnPropertyChanged(); } }

        private bool isGroup { get; set; }
        public bool IsGroup { get => isGroup; set { isGroup = value; OnPropertyChanged(); } }

        private bool isContact { get; set; }
        public bool IsContact { get => isContact; set { isContact = value; OnPropertyChanged(); } }

        private bool hasDraft { get; set; }
        public bool HasDraft { get => hasDraft; set { hasDraft = value; OnPropertyChanged(); } }

        private bool isSender { get; set; }
        public bool IsSender { get => isSender; set { isSender = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
