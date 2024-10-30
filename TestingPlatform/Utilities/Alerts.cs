namespace TestingPlatform.Utilities
{
    public static class Alerts
    {
        public static Task ShowAsync(string title, string message, string cancel) =>
            Shell.Current.CurrentPage.DisplayAlert(title ?? string.Empty, message ?? string.Empty, cancel ?? string.Empty);
    }
}