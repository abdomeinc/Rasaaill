using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Client.WPF.Helpers;

namespace Client.WPF.UI
{
    public class EmojiControl : ContentControl
    {
        public static readonly DependencyProperty EmojiCodeProperty =
            DependencyProperty.Register("EmojiCode", typeof(string), typeof(EmojiControl),
                new PropertyMetadata(null, OnEmojiCodeChanged));

        public string EmojiCode
        {
            get { return (string)GetValue(EmojiCodeProperty); }
            set { SetValue(EmojiCodeProperty, value); }
        }

        private static async void OnEmojiCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // IMPORTANT: Make the callback 'async void'. This is generally okay for
            // event handlers and dependency property callbacks where you don't need
            // to await the completion of the handler itself.
            var control = (EmojiControl)d;
            var emojiCode = (string)e.NewValue;

            if (!string.IsNullOrEmpty(emojiCode))
            {
                // Call the instance method to load the emoji asynchronously
                await control.LoadEmoji(emojiCode);
            }
            else
            {
                // Clear the content if the emoji code is null or empty
                control.Content = null;
            }
        }

        /// <summary>
        /// Loads the emoji asynchronously and updates the control's content.
        /// </summary>
        /// <param name="emojiCode">The code for the emoji to load.</param>
        private async Task LoadEmoji(string emojiCode)
        {
            // Get the DrawingGroup asynchronously from your EmojiManager
            DrawingGroup? emojiDrawing = await EmojiManager.GetEmojiDrawingAsync(emojiCode);

            // Ensure we are on the UI thread before updating UI properties.
            // Dispatcher.Invoke is safe, but Dispatcher.BeginInvoke might be preferred
            // for non-critical updates to avoid blocking.
            // However, after an 'await', the code typically returns to the original
            // synchronization context (UI thread in this case), so `Dispatcher.Invoke`
            // might not always be strictly necessary if the await happened on the UI thread.
            // It's good practice to be explicit for UI updates originating from background tasks.
            this.Dispatcher.Invoke(() =>
            {
                if (emojiDrawing != null)
                {
                    // Create an Image control with the DrawingImage source
                    this.Content = new Image
                    {
                        Source = new DrawingImage(emojiDrawing),
                        Stretch = Stretch.Uniform // Or Stretch.Fill, depending on your needs
                    };
                }
                else
                {
                    // Optionally set a fallback content if emoji failed to load
                    this.Content = null; // Or a TextBlock with a question mark, etc.
                }
            });
        }
    }
}
