using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Components
{
    public partial class LocalizationMenu : ContentView, IDisposable
    {
        private readonly ILocalizationService _localization;

        private bool disposedValue;

        private bool _isLocalizationMenuVisible { get; set; } = false;

        public LocalizationMenu(ILocalizationService localization)
        {
            ValidateConstructorArguments(localization);
            _localization = localization;
            InitializeComponent();
        }

        public void SubscribeToEvents()
        {
            LanguageChangeButton.Clicked += OnLanguageButtonClicked;
            UALocalizationButton.Clicked += OnLocalizationSelected;
            USLocalizationButton.Clicked += OnLocalizationSelected;
        }

        private static void ValidateConstructorArguments(ILocalizationService localization)
        {
            ArgumentNullException.ThrowIfNull(localization, nameof(localization));
        }

        private async void OnLanguageButtonClicked(object? sender, EventArgs e)
        {
            if (!_isLocalizationMenuVisible)
            {
                LocalizationMenuStack.IsVisible = true;
                LocalizationMenuStack.Opacity = 0;
                await Task.WhenAll(
                    LocalizationMenuStack.TranslateTo(0, 34, 250, Easing.CubicIn),
                    LocalizationMenuStack.FadeTo(1, 250)
                );
                _isLocalizationMenuVisible = true;
            }
            else
            {
                await Task.WhenAll(
                    LocalizationMenuStack.TranslateTo(0, -34, 250, Easing.CubicIn),
                    LocalizationMenuStack.FadeTo(0, 250)
                );
                LocalizationMenuStack.IsVisible = false;
                _isLocalizationMenuVisible = false;
            }
        }

        private void OnLocalizationSelected(object? sender, EventArgs e)
        {
            if (sender is not ImageButton button)
                return;

            string? cultureCode = button.CommandParameter.ToString();

            if (!string.IsNullOrEmpty(cultureCode))
            {
                _localization.ChangeCulture(cultureCode);
            }

            OnLanguageButtonClicked(sender, e);
        }

        #region Disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (LanguageChangeButton != null)
                        LanguageChangeButton.Clicked -= OnLanguageButtonClicked;

                    if (UALocalizationButton != null)
                        UALocalizationButton.Clicked -= OnLocalizationSelected;

                    if (USLocalizationButton != null)
                        USLocalizationButton.Clicked -= OnLocalizationSelected;
                }

                disposedValue = true;
            }
        }

        //~LocalizationMenu()
        //{
        //    Dispose(disposing: false);
        //}

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}