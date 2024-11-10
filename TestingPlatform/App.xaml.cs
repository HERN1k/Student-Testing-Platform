using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform
{
    public partial class App : Application
    {
        public readonly ILocalizationService _localization;

        public App(ILocalizationService localization)
        {
            ValidateConstructorArguments(localization);
            _localization = localization;
            _localization.SetCultureOnStartup();
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            ArgumentNullException.ThrowIfNull(activationState);

            Window window = base.CreateWindow(activationState);
            window.Title = Constants.AppName;
            return window;
        }

        private static void ValidateConstructorArguments(ILocalizationService localization)
        {
            ArgumentNullException.ThrowIfNull(localization, nameof(localization));
        }
    }
}