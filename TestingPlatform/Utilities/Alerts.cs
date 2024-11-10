using System.Text;

using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Utilities
{
    public static class Alerts
    {
        public static Task ShowAsync(string title, string message, string cancel)
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null)
            {
                return Task.CompletedTask;
            }

            return currentPage.DisplayAlert(title ?? string.Empty, message ?? string.Empty, cancel ?? string.Empty);
        }

        public static Task ShowErrorAsync(Exception ex, ILocalizationService? localization = null)
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null)
            {
                return Task.CompletedTask;
            }

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

            return currentPage.DisplayAlert(title, sb.ToString(), cancel);
        }

        public static Task ShowErrorWithTraceAsync(Exception ex, ILocalizationService? localization = null)
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null)
            {
                return Task.CompletedTask;
            }

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

            return currentPage.DisplayAlert(title, sb.ToString(), cancel);
        }

        public static async Task ToggleLoader()
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null)
            {
                return;
            }

            var loader = Shell.Current.CurrentPage.FindByName<ActivityIndicator>("Loader");
            var border = Shell.Current.CurrentPage.FindByName<Border>("LoaderBG");

            if (loader != null && border != null)
            {
                if (loader.IsVisible)
                {
                    await Task.WhenAll(
                        loader.FadeTo(0, 250, Easing.CubicIn),
                        border.FadeTo(0, 250, Easing.CubicIn)
                    );
                    loader.IsVisible = false;
                    border.IsVisible = false;
                }
                else
                {
                    loader.IsVisible = true;
                    border.IsVisible = true;
                    await Task.WhenAll(
                        loader.FadeTo(1, 250, Easing.CubicIn),
                        border.FadeTo(0.5, 250, Easing.CubicIn)
                    );
                }
            }
        }

        public static async Task ToggleLoader(bool isVisible)
        {
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage == null)
            {
                return;
            }

            var loader = Shell.Current.CurrentPage.FindByName<ActivityIndicator>("Loader");
            var border = Shell.Current.CurrentPage.FindByName<Border>("LoaderBG");

            if (loader != null && border != null)
            {
                if (loader.IsVisible != isVisible)
                {
                    if (isVisible)
                    {
                        loader.IsVisible = true;
                        border.IsVisible = true;
                        await Task.WhenAll(
                            loader.FadeTo(1, 250, Easing.CubicIn),
                            border.FadeTo(0.5, 250, Easing.CubicIn)
                        );
                    }
                    else
                    {
                        await Task.WhenAll(
                            loader.FadeTo(0, 250, Easing.CubicIn),
                            border.FadeTo(0, 250, Easing.CubicIn)
                        );
                        loader.IsVisible = false;
                        border.IsVisible = false;
                    }
                }
            }
        }
    }
}