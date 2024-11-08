using System.Globalization;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Components
{
    public partial class HomeView : ContentView, IDisposable
    {
        private readonly ILocalizationService _localization;
        private readonly IGraphService _graph;

        private bool disposedValue;

        public HomeView(ILocalizationService localization, IGraphService graph)
        {
            _localization = localization;
            _graph = graph;

            InitializeComponent();
            MainThread.InvokeOnMainThreadAsync(() => SetUserProfileDataAsync());
        }

        public void SubscribeToEvents()
        { }

        public async Task SetUserProfileDataAsync()
        {
            UserProfileEmailLabel.Text = Preferences.Get(Constants.UserMail, "Null");
            UserProfileNameLabel.Text = Preferences.Get(Constants.UserDisplayName, "Null");

            byte[] imageBytes = await _graph.GetMyPhotoAsync();

            if (imageBytes.Length == 0)
            {
                UserProfileImage.Source = "user.png";
                return;
            }

            try
            {
                UserProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes) { Position = 0 });
            }
            catch (Exception)
            {
                UserProfileImage.Source = "user.png";
            }
        }

        private void OnCultureChanged(object? sender, CultureInfo newCulture)
        {
            UpdateTexts(newCulture);
        }

        private void UpdateTexts(CultureInfo culture)
        {
            // HomeButtonLabel.Text = _localization.GetString("Home", culture);
        }

        #region Disposing
        ~HomeView()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}