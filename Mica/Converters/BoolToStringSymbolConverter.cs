using System.Globalization;
using System.Windows.Data;

namespace Mica.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStringSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "✅" : "❌";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
