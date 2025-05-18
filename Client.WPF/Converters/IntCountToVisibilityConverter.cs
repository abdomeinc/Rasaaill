using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.WPF.Converters
{
    public class IntCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int count ? count > 0 ? Visibility.Visible : Visibility.Collapsed : (object)Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility ? visibility == Visibility.Visible : (object)false;
        }
    }
}
