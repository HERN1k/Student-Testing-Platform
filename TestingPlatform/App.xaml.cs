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

            MainPage = new AppShell()
            {
                Title = Constants.AppName
            };
        }

        private static void ValidateConstructorArguments(ILocalizationService localization)
        {
            ArgumentNullException.ThrowIfNull(localization, nameof(localization));
        }
    }
}