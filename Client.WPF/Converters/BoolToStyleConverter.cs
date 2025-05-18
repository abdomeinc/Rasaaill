using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.WPF.Converters
{
    public class BoolToStyleConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSender && parameter is string styles)
            {
                string[] styleNames = styles.Split(';');
                return Application.Current.FindResource(isSender ? styleNames[0] : styleNames[1]);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
