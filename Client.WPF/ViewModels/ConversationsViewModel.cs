using System.Collections.ObjectModel;

namespace Client.WPF.ViewModels
{
    public class ConversationsViewModel : ViewModelBase
    {
        private ObservableCollection<ConversationViewModel> _conversations = new();

        public ObservableCollection<ConversationViewModel> Conversations
        {
            get => _conversations;
            set
            {
                if (_conversations != value)
                {
                    _conversations = value;
                    OnPropertyChanged();
                }
            }
        }
        private ConversationViewModel? _selectedConversation;

        public ConversationViewModel? SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                if (_selectedConversation != value)
                {
                    _selectedConversation = value;
                    OnPropertyChanged();
                    if (_selectedConversation != null)
                        SwitchToConversation(_selectedConversation);
                }
            }
        }

        public ConversationsViewModel()
        {

            _ = LoadConversationsAsync();
        }

        public void SwitchToConversation(ConversationViewModel conversation)
        {
            // Optionally, set a property or command to display the selected conversation
            // This could be handled inside ConversationsViewModel as well
            // For now, just set SelectedConversation
            SelectedConversation = conversation;
        }

        private async Task LoadConversationsAsync()
        {
            // TODO: Load conversations from API/service
            // For now, add mock data
            await Task.Delay(100); // Simulate async
            Conversations = new ObservableCollection<ConversationViewModel>
            {
                new ConversationViewModel { Id = Guid.NewGuid(), Title = "General" },
                new ConversationViewModel { Id = Guid.NewGuid(), Title = "Random" }
            };
        }
    }
}
