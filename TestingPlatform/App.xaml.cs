using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform
{
    public partial class App : Application
    {
        public readonly IAuthenticationService _authentication;
        public readonly ILocalizationService _localization;

        public App(IAuthenticationService authentication, ILocalizationService localization)
        {
            _authentication = authentication;
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
    }
}