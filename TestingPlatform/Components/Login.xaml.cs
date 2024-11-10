using System.Globalization;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Components
{
    public partial class Login : ContentView, IDisposable
    {
        private readonly IGraphService _graph;
        private readonly IApiService _api;
        private readonly ILocalizationService _localization;
        private bool IsPassword { get; set; } = true;
        private bool disposedValue;

        public Login(IGraphService graph, IApiService api, ILocalizationService localization)
        {
            ValidateConstructorArguments(graph, api, localization);
            _graph = graph;
            _api = api;
            _localization = localization;
            InitializeComponent();
        }

        public void SubscribeToEvents()
        {
            _localization.CultureChanged += OnCultureChanged;
            EmailEntry.TextChanged += OnEmailChanged;
            PasswordEntry.TextChanged += OnPasswordChanged;
            MicrosoftButton.Clicked += OnSignInMicrosoft;
            SubmitButton.Clicked += Submit;
        }

        private static void ValidateConstructorArguments(IGraphService graph, IApiService api, ILocalizationService localization)
        {
            ArgumentNullException.ThrowIfNull(graph, nameof(graph));
            ArgumentNullException.ThrowIfNull(api, nameof(api));
            ArgumentNullException.ThrowIfNull(localization, nameof(localization));
        }

        private void OnEmailChanged(object? sender, TextChangedEventArgs e)
        {
            bool isValid = StringHelper.ValidateEmail(e.NewTextValue, EmailErrorLabel, _localization);

            if (isValid)
            {
                EmailErrorLabel.IsVisible = false;
            }
        }

        private void OnPasswordChanged(object? sender, TextChangedEventArgs e)
        {
            bool isValid = StringHelper.ValidatePassword(e.NewTextValue, PasswordErrorLabel, _localization);

            if (isValid)
            {
                PasswordErrorLabel.IsVisible = false;
            }
        }

        private void ChangePasswordVisiblity(object? sender, EventArgs e)
        {
            if (sender is not ImageButton button)
                return;

            IsPassword = !IsPassword;
            PasswordEntry.IsPassword = IsPassword;

            if (IsPassword)
                button.Source = "eye.png";
            else
                button.Source = "eye_open.png";
        }

        private async void Submit(object? sender, EventArgs e)
        {
            bool isValidEmail = StringHelper.ValidateEmail(EmailEntry.Text, EmailErrorLabel, _localization);
            bool isValidPassword = StringHelper.ValidatePassword(PasswordEntry.Text, PasswordErrorLabel, _localization);

            if (!isValidEmail || !isValidPassword)
                return;

            EmailEntry.Text = string.Empty;
            EmailErrorLabel.IsVisible = false;
            PasswordEntry.Text = string.Empty;
            PasswordErrorLabel.IsVisible = false;

            await Alerts.ShowAsync(_localization.GetString("Congratulations"), _localization.GetString("SignInWithMicrosoftAccount"), _localization.GetString("OK"));
        }

        private async void OnSignInMicrosoft(object? sender, EventArgs e)
        {
            try
            {
                await _graph.AuthenticationAsync();
                await _api.AuthenticationAsync();
            }
            catch (Exception ex)
            {
#if DEBUG
                await Alerts.ShowErrorWithTraceAsync(ex, _localization);
#else
                await Alerts.ShowErrorAsync(ex, _localization);
#endif
                return;
            }

            try
            {
                await Shell.Current.GoToAsync("Home");
            }
            catch (Exception ex)
            {
#if DEBUG
                await Alerts.ShowErrorWithTraceAsync(ex, _localization);
#else
                await Alerts.ShowErrorAsync(ex, _localization);
#endif
                throw new NotImplementedException();
            }
        }

        private void OnCultureChanged(object? sender, CultureInfo newCulture)
        {
            UpdateTexts(newCulture);
        }

        private void UpdateTexts(CultureInfo culture)
        {
            EmailLabel.Text = _localization.GetString("Email", culture);
            PasswordLabel.Text = _localization.GetString("Password", culture);
            SubmitButton.Text = _localization.GetString("SignIn", culture);

            StringHelper.ValidateEmail(EmailEntry.Text, EmailErrorLabel, _localization);
            StringHelper.ValidatePassword(PasswordEntry.Text, PasswordErrorLabel, _localization);
        }

        #region Disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (PasswordEntry != null)
                        PasswordEntry.TextChanged -= OnPasswordChanged;

                    if (EmailEntry != null)
                        EmailEntry.TextChanged -= OnEmailChanged;

                    if (MicrosoftButton != null)
                        MicrosoftButton.Clicked -= OnSignInMicrosoft;

                    if (SubmitButton != null)
                        SubmitButton.Clicked -= Submit;

                    _localization.CultureChanged -= OnCultureChanged;
                }

                disposedValue = true;
            }
        }

        //~Login()
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