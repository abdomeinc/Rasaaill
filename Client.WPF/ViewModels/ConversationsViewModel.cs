using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Client.WPF.ViewModels
{
    public partial class ConversationsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConversationsViewModel> _logger;
        private readonly Services.Interfaces.IUserStore _userStore;
        private readonly Services.Interfaces.IConversationGeneratorService _conversationGeneratorService;

        [ObservableProperty]
        private ObservableCollection<Entities.Dtos.ConversationDto> conversations = [];

        [ObservableProperty]
        private ObservableCollection<Entities.Dtos.ConversationDto> filteredConversations = [];

        [ObservableProperty]
        private Entities.Dtos.ConversationDto? selectedConversation;

        [ObservableProperty]
        private ConversationViewModel? currentConversation;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private Shared.ConversationFilterType selectedFilter = Shared.ConversationFilterType.All;

        public ConversationsViewModel(IServiceProvider serviceProvider, ILogger<ConversationsViewModel> logger, Services.Interfaces.IUserStore userStore, Services.Interfaces.IConversationGeneratorService conversationGeneratorService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _userStore = userStore;
            _conversationGeneratorService = conversationGeneratorService;
            _ = Task.Run(async () =>
            {
                try { await LoadConversationsAsync(); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to load conversations."); }
            });

        }

        public void SwitchToConversation(Entities.Dtos.ConversationDto conversation)
        {
            // Optionally, set a property or command to display the selected conversation
            // This could be handled inside ConversationsViewModel as well
            // For now, just set SelectedConversation
            //SelectedConversation = conversation;
        }

        private Task LoadConversationsAsync()
        {
            var generated = _conversationGeneratorService.GenerateConversations(2, 1);

            Conversations = [.. generated.OrderByDescending(e => e.LastMessage?.Timestamp)];

            return Task.CompletedTask;

            //SetFilter();
        }

        [RelayCommand]
        private void SetFilter()
        {
            IEnumerable<Entities.Dtos.ConversationDto> filtered = Conversations.AsEnumerable();
            SelectedConversation = null;

            // Filter
            switch (SelectedFilter)
            {
                case Shared.ConversationFilterType.Unread:
                    filtered = filtered.Where(c => c.UnreadCount > 0);
                    break;
                case Shared.ConversationFilterType.Favorites:
                    filtered = filtered.Where(c => c.IsFavorite);
                    break;
                case Shared.ConversationFilterType.Contacts:
                    filtered = filtered.Where(c => !c.IsGroup && c.IsContact);
                    break;
                case Shared.ConversationFilterType.NonContacts:
                    filtered = filtered.Where(c => !c.IsGroup && !c.IsContact);
                    break;
                case Shared.ConversationFilterType.Groups:
                    filtered = filtered.Where(c => c.IsGroup);
                    break;
                case Shared.ConversationFilterType.Drafts:
                    filtered = filtered.Where(c => c.HasDraft);
                    break;
            }


            // Search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c => c.DisplayName != null && c.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            FilteredConversations = [.. filtered];
            OnPropertyChanged(nameof(FilteredConversations));
        }

        partial void OnSearchTextChanged(string value)
        {
            SetFilter();
        }

        partial void OnSelectedFilterChanged(Shared.ConversationFilterType value)
        {
            SetFilter();
        }

        partial void OnConversationsChanged(ObservableCollection<Entities.Dtos.ConversationDto> value)
        {
            SetFilter();
        }

        partial void OnSelectedConversationChanged(ConversationDto? value)
        {
            if (value is null)
            {
                ClearSelection();
                return;
            }

            DeselectAllConversationsExcept(value);
            value.IsSelected = true;
            _userStore.StoredConversation = value;

            LoadCurrentConversationViewModel(value);
        }

        private void ClearSelection()
        {
            _userStore.StoredConversation = null;
            CurrentConversation = null;
        }

        private void DeselectAllConversationsExcept(ConversationDto selected)
        {
            foreach (var conv in Conversations)
                conv.IsSelected = conv == selected;

            foreach (var conv in FilteredConversations)
                conv.IsSelected = conv == selected;
        }

        private void LoadCurrentConversationViewModel(ConversationDto selected)
        {
            CurrentConversation = _serviceProvider.GetRequiredService<ConversationViewModel>();

            CurrentConversation.OnNewMessage += message =>
            {
                var target = Conversations.FirstOrDefault(c => c.Id == message.ConversationId);
                if (target is not null)
                {
                    target.Messages.Add(message);
                    target.LastMessage = message;
                }

                SortConversations(target);
            };
        }

        //private void SortConversations(ConversationDto? conversationToBeSelected)
        //{
        //    Conversations = [.. Conversations.OrderByDescending(c => c.LastMessage?.Timestamp)];

        //    FilteredConversations = [.. FilteredConversations.OrderByDescending(c => c.LastMessage?.Timestamp)];
        //}
        private void SortConversations(ConversationDto? conversationToBeSelected)
        {
            var sorted = Conversations.OrderByDescending(c => c.LastMessage?.Timestamp).ToList();

            // Clear and repopulate the ObservableCollection to ensure UI refresh
            Conversations.Clear();
            foreach (var conversation in sorted)
            {
                Conversations.Add(conversation);
            }

            // Same for FilteredConversations if needed (make sure it’s ObservableCollection)
            var sortedFiltered = FilteredConversations.OrderByDescending(c => c.LastMessage?.Timestamp).ToList();
            FilteredConversations.Clear();
            foreach (var conversation in sortedFiltered)
            {
                FilteredConversations.Add(conversation);
            }

            // Set SelectedConversation explicitly after sorting
            if (conversationToBeSelected != null)
            {
                SelectedConversation = Conversations.FirstOrDefault(c => c.Id == conversationToBeSelected.Id);
            }
            else
            {
                SelectedConversation = Conversations.FirstOrDefault();
            }
        }
    }
}
