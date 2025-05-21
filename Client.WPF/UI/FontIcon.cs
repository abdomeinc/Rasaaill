using Client.WPF.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace Client.WPF.UI
{
    public class FontIcon : Control
    {
        public IconsLoader.Icon Icon
        {
            get => (IconsLoader.Icon)GetValue(IconProperty);
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconsLoader.Icon), typeof(FontIcon), new PropertyMetadata(new PropertyChangedCallback(IconChangedCallback)));

        public string IconSymbol
        {
            get => (string)GetValue(IconSymbolProperty);
            set
            {
                SetValue(IconSymbolProperty, value);
            }
        }

        public static readonly DependencyProperty IconSymbolProperty = DependencyProperty.Register("IconSymbol", typeof(string), typeof(FontIcon));


        private static void IconChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FontIcon ctl = (FontIcon)obj;
            int newValue = (int)args.NewValue;

            ctl.IconSymbol = IconsLoader.GetIconSymbol((IconsLoader.Icon)newValue);//GridView Icon -> this result \\xF0E2 (but doesn't draw the icon)
            //ctl.IconSymbol = "\xF0E2";//this the same but draws the icon
        }
    }
}
