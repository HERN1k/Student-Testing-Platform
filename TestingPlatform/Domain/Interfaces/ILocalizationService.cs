using System.Globalization;

namespace TestingPlatform.Domain.Interfaces
{
    public interface ILocalizationService
    {
        event EventHandler<CultureInfo> CultureChanged;
        CultureInfo CurrentCulture { get; }
        string GetString(string key, CultureInfo? culture = null);
        void SetCultureOnStartup();
        void ChangeCulture(string cultureCode);
    }
}