using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.WPF.Models
{
    public partial class NavigationItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private Helpers.IconsLoader.Icon _icon = Helpers.IconsLoader.Icon.Home;

        [ObservableProperty]
        private bool _isSelected = false;

        [ObservableProperty]
        private Type? _dataType;
    }
}
