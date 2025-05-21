using Client.WPF.UI;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for EmojiPickerView.xaml
    /// </summary>
    public partial class EmojiPickerView : UserControl
    {
        public EmojiPickerView()
        {
            InitializeComponent();
        }

        // In your MainWindow or a custom control
        public static void RenderEmojisInTextBlock(TextBlock textBlock)
        {
            string text = textBlock.Text;
            textBlock.Inlines.Clear(); // Clear existing content

            // Simple example for shortcodes (you'd need a more robust parser)
            string[] parts = text.Split(' '); // Split by spaces

            foreach (string part in parts)
            {
                if (part.StartsWith(":") && part.EndsWith(":"))
                {
                    string shortcode = part;
                    // Lookup the EmojiCode from the shortcode (you'd need a mapping)
                    string emojiCode = GetEmojiCodeFromShortcode(shortcode);
                    if (!string.IsNullOrEmpty(emojiCode))
                    {
                        var emojiControl = new EmojiControl { EmojiCode = emojiCode, Width = 20, Height = 20 };
                        textBlock.Inlines.Add(new InlineUIContainer(emojiControl));
                    }
                    else
                    {
                        textBlock.Inlines.Add(new Run(part + " ")); // Display as text if not a known shortcode
                    }
                }
                else
                {
                    textBlock.Inlines.Add(new Run(part + " "));
                }
            }
        }

        // You'd need a way to map shortcodes to EmojiCodes
        private static string GetEmojiCodeFromShortcode(string shortcode)
        {
            // Implement your lookup logic here (e.g., from a dictionary)
            if (shortcode == ":grinning_face:") return "u1f600";
            // ... other mappings
            return "null";
        }

        private void EmojiControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
