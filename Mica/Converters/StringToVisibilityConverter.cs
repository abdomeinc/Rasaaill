using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mica.Converters
{
    /// <summary>
    /// Converts a string value to a Visibility value.
    /// Visible if the string is not null or whitespace, Collapsed otherwise.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a string and is not null, empty, or whitespace
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                // If it's a non-empty string, return Visible
                return Visibility.Visible;
            }

            // Otherwise, return Collapsed (element is hidden and doesn't take up layout space)
            return Visibility.Collapsed;

            // Use Visibility.Hidden instead of Collapsed if you want the element to remain
            // in the layout but be invisible. Collapsed is usually preferred for error messages.
            // return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Converting back from Visibility to string is not needed for this scenario
            throw new NotImplementedException();
        }
    }
}
