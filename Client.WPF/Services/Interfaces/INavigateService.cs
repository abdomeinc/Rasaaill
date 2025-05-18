namespace Client.WPF.Services.Interfaces
{
    public interface INavigateService
    {
        event Action<Shared.ScreenType>? OnScreenChanged;
        void NavigateTo(Shared.ScreenType screen);
    }
}
