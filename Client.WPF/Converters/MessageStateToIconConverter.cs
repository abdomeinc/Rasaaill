using System.Globalization;
using System.Windows.Data;

namespace Client.WPF.Converters
{
    [ValueConversion(typeof(Shared.MessageState), typeof(Helpers.IconsLoader.Icon))]
    public class MessageStateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Shared.MessageState.Unknown => Helpers.IconsLoader.Icon.Help,// Placeholder for unknown state
                Shared.MessageState.Deleted => Helpers.IconsLoader.Icon.Cancel,// Icon for deleted message
                Shared.MessageState.Sending => Helpers.IconsLoader.Icon.Stopwatch,// Icon for sending message
                Shared.MessageState.Sent => Helpers.IconsLoader.Icon.CheckMark,// Icon for sent message
                Shared.MessageState.Received => Helpers.IconsLoader.Icon.StatusCheckmark,// Icon for received message
                Shared.MessageState.Seen => Helpers.IconsLoader.Icon.RedEye,// Icon for seen message (distinguishable from received)
                _ => (object)Helpers.IconsLoader.Icon.Help,// Fallback icon for unknown state
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Only need to convert forward
        }
    }
}
