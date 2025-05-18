using System.Windows;

namespace Client.WPF.Services
{
    public class NavigateService : Interfaces.INavigateService
    {
        /// <summary>
        /// Service which provides the instances of pages.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        public event Action<Shared.ScreenType>? OnScreenChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigateService"/> class and attaches the <see cref="IServiceProvider"/>.
        /// </summary>
        public NavigateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo(Shared.ScreenType screen)
        {
            OnScreenChanged?.Invoke(screen);
        }

        /// <inheritdoc />
        public T? GetPage<T>()
            where T : class
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException("The page should be a WPF control.");
            }

            return (T?)_serviceProvider.GetService(typeof(T));
        }

        /// <inheritdoc />
        public FrameworkElement? GetPage(Type pageType)
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
            {
                throw new InvalidOperationException("The page should be a WPF control.");
            }

            return _serviceProvider.GetService(pageType) as FrameworkElement;
        }
    }
}
