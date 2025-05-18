using System.Globalization;
using System.Windows.Data;

namespace Client.WPF.Converters
{
    public class BubbleColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMine = (bool)value;
            return isMine ? 2 : 0; // mine = right, other = left
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
