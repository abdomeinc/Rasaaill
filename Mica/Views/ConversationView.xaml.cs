using System.Collections.Specialized;
using System.Windows.Controls;

namespace Mica.Views
{
    /// <summary>
    /// Interaction logic for ConversationView.xaml
    /// </summary>
    public partial class ConversationView : UserControl
    {
        public ConversationView()
        {
            InitializeComponent();

            Loaded += ConversationView_Loaded;
        }

        private void ConversationView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Also try once at load
            SubscribeToMessagesCollection();
            ScrollToBottom();
        }


        private void SubscribeToMessagesCollection()
        {
            if (DataContext is ViewModels.ConversationViewModel vm && vm.Conversation?.Messages is INotifyCollectionChanged messages)
            {
                messages.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Scroll to bottom on UI thread
                Dispatcher.InvokeAsync(() =>
                {
                    ScrollToBottom();
                });
            }
        }

        private void ScrollToBottom()
        {
            if (MessagesListBox.Items.Count == 0) return;

            var lastItem = MessagesListBox.Items[MessagesListBox.Items.Count - 1];
            MessagesListBox.ScrollIntoView(lastItem);
        }
    }
}
