using System.Text;

using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Utilities
{
    public static class Alerts
    {
        public static Task ShowAsync(string title, string message, string cancel) =>
            Shell.Current.CurrentPage.DisplayAlert(title ?? string.Empty, message ?? string.Empty, cancel ?? string.Empty);

        public static Task ShowErrorAsync(Exception ex, ILocalizationService? localization = null)
        {
            StringBuilder sb = new();
            string title;
            string cancel;
            if (localization == null)
            {
                title = "Error";
                cancel = "OK";
                sb.Append("Message:\t");
                sb.Append(ex.Message);
            }
            else
            {
                title = localization.GetString("Error") ?? string.Empty;
                cancel = localization.GetString("OK") ?? string.Empty;
                sb.Append("Message:\t");
                sb.Append(ex.Message);
            }

            return Shell.Current.CurrentPage.DisplayAlert(title, sb.ToString(), cancel);
        }

        public static Task ShowErrorWithTraceAsync(Exception ex, ILocalizationService? localization = null)
        {
            StringBuilder sb = new();
            string title;
            string cancel;
            if (localization == null)
            {
                title = "Error";
                cancel = "OK";
                sb.Append("Message:\t");
                sb.Append(ex.Message);
                sb.Append("\n\t");
                sb.Append(ex.StackTrace);
            }
            else
            {
                title = localization.GetString("Error") ?? string.Empty;
                cancel = localization.GetString("OK") ?? string.Empty;
                sb.Append("Message:\t");
                sb.Append(ex.Message);
                sb.Append("\n\t");
                sb.Append(ex.StackTrace);
            }

            return Shell.Current.CurrentPage.DisplayAlert(title, sb.ToString(), cancel);
        }
    }
}