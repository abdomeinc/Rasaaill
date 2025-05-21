using Client.WPF.Helpers;
using Client.WPF.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.WPF.ViewModels
{
    public partial class EmojiPickerViewModel : ViewModelBase
    {
        // Holds ALL emoji metadata, but not their SVG content
        [ObservableProperty]
        private ObservableCollection<Models.Emoji>? _allEmojis;

        // The view that the UI will bind to, allowing for filtering and sorting
        /// <summary>
        /// The collection of emojis that the UI will display, potentially filtered.
        /// </summary>
        [ObservableProperty]
        private ICollectionView? _filteredEmojis;

        [ObservableProperty]
        private string? _searchText;



        // Optionally, you might want to immediately publish this selection
        // if the picker closes on selection, or trigger a command.
        // For now, just setting the property is enough.
        [ObservableProperty]
        private Emoji? _selectedEmoji; // Make it nullable if nothing is selected initially

        public EmojiPickerViewModel()
        {
            _allEmojis = new ObservableCollection<Models.Emoji>();

            // Initialize the CollectionViewSource to enable filtering and sorting
            _filteredEmojis = CollectionViewSource.GetDefaultView(_allEmojis);
            _filteredEmojis.Filter = FilterEmojis; // Apply our filter logic

            // Start loading the emoji metadata asynchronously when the ViewModel is created
            LoadEmojiMetadata();
        }

        partial void OnSearchTextChanged(string? value)
        {
            // When search text changes, refresh the filter on the collection view
            FilteredEmojis?.Refresh();
        }

        /// <summary>
        /// Asynchronously loads all emoji metadata from the EmojiManager.
        /// </summary>
        private void LoadEmojiMetadata()
        {
            // This call gets the list of Emoji objects with their basic metadata
            // but without their DrawingGroup loaded yet.
            //var metadata = await EmojiManager.LoadAllEmojiMetadataAsync();

            // Add them to the ObservableCollection on the UI thread
            // (Task.Run in LoadAllEmojiMetadataAsync ensured loading was on background,
            // but ObservableCollection updates need to be on UI thread or use a dispatcher)
            // Since this is typically called once on ViewModel init, direct add is fine.
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var emoji in Helpers.EmojiManager._emojisList)
                {
                    AllEmojis?.Add(emoji);
                }
            });
        }

        /// <summary>
        /// Filter predicate for the ICollectionView.
        /// </summary>
        private bool FilterEmojis(object item)
        {
            // If no search text, show all emojis
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            // Cast the item to an Emoji object
            var emoji = item as Models.Emoji;
            if (emoji == null) return false;

            // Perform case-insensitive search on description and code
            return emoji.Description.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   emoji.Code.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
