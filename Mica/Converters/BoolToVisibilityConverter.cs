﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mica.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is "!", invert the logic
                bool invert = parameter?.ToString() == "!";
                if (invert)
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility ? visibility == Visibility.Visible : (object)false;
        }
    }
}
