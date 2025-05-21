using System.Collections.Specialized;
using System.Windows.Controls;

namespace Client.WPF.Views
{
    public partial class ConversationView : UserControl
    {
        public ConversationView()
        {
            InitializeComponent();

            Loaded += ConversationView_Loaded;
        }

        private void ConversationView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
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
                _ = Dispatcher.InvokeAsync(ScrollToBottom);
            }
        }

        private void ScrollToBottom()
        {
            if (MessagesListBox.Items.Count == 0)
            {
                return;
            }

            object lastItem = MessagesListBox.Items[^1];
            MessagesListBox.ScrollIntoView(lastItem);
        }

        //private void Picker_Picked(object sender, Emoji.Wpf.EmojiPickedEventArgs e)
        //{
        //    EmojiRichTextBox.CaretPosition.InsertTextInRun(e.Emoji);
        //}
    }
}
