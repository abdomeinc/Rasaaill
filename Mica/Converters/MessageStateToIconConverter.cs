using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;

namespace Mica.Converters
{
    [ValueConversion(typeof(Shared.MessageState), typeof(PackIconKind))]
    public class MessageStateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Shared.MessageState.Unknown => PackIconKind.HelpCircle,// Placeholder for unknown state
                Shared.MessageState.Deleted => PackIconKind.Cancel,// Icon for deleted message
                Shared.MessageState.Sending => PackIconKind.ClockOutline,// Icon for sending message
                Shared.MessageState.Sent => PackIconKind.Check,// Icon for sent message
                Shared.MessageState.Received => PackIconKind.CheckAll,// Icon for received message
                Shared.MessageState.Seen => PackIconKind.CheckCircleOutline,// Icon for seen message (distinguishable from received)
                _ => (object)PackIconKind.HelpCircle,// Fallback icon for unknown state
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Only need to convert forward
        }
    }
}
