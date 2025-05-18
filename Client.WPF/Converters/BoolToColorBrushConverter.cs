using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    internal class BoolToColorBrushConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Bolean value controlling wether to apply color change</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">A CSV string on the format [ColorNameIfTrue;ColorNameIfFalse;OpacityNumber] may be provided for customization, default is [LimeGreen;Transperent;1.0].</param>
        /// <param name="culture"></param>
        /// <returns>A SolidColorBrush in the supplied or default colors depending on the state of value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color;
            // Setting default values
            Color colorIfTrue = Colors.LimeGreen;
            Color colorIfFalse = Colors.DarkRed;
            double opacity = 1;
            // Parsing converter parameter
            if (parameter != null)
            {
                // Parameter format: [ColorNameIfTrue;ColorNameIfFalse;OpacityNumber]
                string? parameterstring = parameter.ToString();
                if (!string.IsNullOrEmpty(parameterstring))
                {
                    string[] parameters = parameterstring.Split(';');
                    int count = parameters.Length;
                    if (count > 0 && !string.IsNullOrEmpty(parameters[0]))
                    {
                        colorIfTrue = ColorFromName(parameters[0]);
                    }
                    if (count > 1 && !string.IsNullOrEmpty(parameters[1]))
                    {
                        colorIfFalse = ColorFromName(parameters[1]);
                    }
                    if (count > 2 && !string.IsNullOrEmpty(parameters[2]))
                    {
                        if (double.TryParse(parameters[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double dblTemp))
                        {
                            opacity = dblTemp;
                        }
                    }
                }
            }
            // Creating Color Brush
            color = (bool)value
                ? new SolidColorBrush(colorIfTrue)
                {
                    Opacity = opacity
                }
                : new SolidColorBrush(colorIfFalse)
                {
                    Opacity = opacity
                };
            return color;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static Color ColorFromName(string colorName)
        {
            System.Drawing.Color systemColor = System.Drawing.Color.FromName(colorName);
            return Color.FromArgb(systemColor.A, systemColor.R, systemColor.G, systemColor.B);
        }
    }
}
