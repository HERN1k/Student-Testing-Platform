using System.Globalization;
using System.Resources;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private CultureInfo _currentCulture;
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            private set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    OnCultureChanged();
                }
            }
        }

        public event EventHandler<CultureInfo> CultureChanged = null!;
        private readonly ResourceManager _resourceManager;

        public LocalizationService()
        {
            _resourceManager = new ResourceManager("TestingPlatform.Resources.Languages.Language", typeof(LocalizationService).Assembly);
            _currentCulture = new(Preferences.Get(Constants.Culture, "uk-UA"));
        }

        public string GetString(string key, CultureInfo? culture = null)
        {
            return _resourceManager.GetString(key, culture ?? CurrentCulture) ?? string.Empty;
        }

        public void SetCultureOnStartup() => ChangeCulture(Preferences.Get(Constants.Culture, "uk-UA"));

        public void ChangeCulture(string cultureCode)
        {
            if (!string.IsNullOrEmpty(cultureCode))
            {
                CurrentCulture = new(cultureCode);
                Preferences.Set(Constants.Culture, cultureCode);
                CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
                CultureInfo.CurrentCulture = CurrentCulture;
                CultureInfo.CurrentUICulture = CurrentCulture;
            }
        }

        protected virtual void OnCultureChanged()
        {
            CultureChanged?.Invoke(this, CurrentCulture);
        }
    }
}