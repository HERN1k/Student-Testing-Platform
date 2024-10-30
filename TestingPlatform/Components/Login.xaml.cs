using System.Diagnostics;
using System.Globalization;

using Microsoft.Identity.Client;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Domain.Json;
using TestingPlatform.Utilities;

namespace TestingPlatform.Components
{
    public partial class Login : ContentView, IDisposable
    {
        private readonly IAuthenticationService _authentication;
        private readonly ILocalizationService _localization;
        private bool disposedValue;

        private bool _isPassword { get; set; } = true;

        public Login(IAuthenticationService authentication, ILocalizationService localization)
        {
            _authentication = authentication;
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

            _isPassword = !_isPassword;
            PasswordEntry.IsPassword = _isPassword;

            if (_isPassword)
                button.Source = "eye.png";
            else
                button.Source = "eye_open.png";
        }

        private async void Submit(object? sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("Home");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //bool isValidEmail = StringHelper.ValidateEmail(EmailEntry.Text, EmailErrorLabel, _localization);
            //bool isValidPassword = StringHelper.ValidatePassword(PasswordEntry.Text, PasswordErrorLabel, _localization);

            //if (!isValidEmail || !isValidPassword)
            //    return;

            //EmailEntry.Text = string.Empty;
            //EmailErrorLabel.IsVisible = false;
            //PasswordEntry.Text = string.Empty;
            //PasswordErrorLabel.IsVisible = false;

            //await Alerts.ShowAsync(_localization.GetString("Congratulations"), _localization.GetString("SignInWithMicrosoftAccount"), _localization.GetString("OK"));
        }

        private async void OnSignInMicrosoft(object? sender, EventArgs e)
        {
            AuthenticationResult? result = await _authentication.SignInAsync();
            if (result != null)
            {
                Me? me = await _authentication.HttpGet<Me>("me");

                if (me is null)
                    return;

                EmailEntry.Text = string.Empty;
                EmailErrorLabel.IsVisible = false;
                PasswordEntry.Text = string.Empty;
                PasswordErrorLabel.IsVisible = false;

                Preferences.Set(Constants.UserDisplayName, me.DisplayName);
                Preferences.Set(Constants.UserGivenName, me.GivenName);
                Preferences.Set(Constants.UserJobTitle, me.JobTitle);
                Preferences.Set(Constants.UserMail, me.Mail);
                Preferences.Set(Constants.UserMobilePhone, me.MobilePhone);
                Preferences.Set(Constants.UserOfficeLocation, me.OfficeLocation);
                Preferences.Set(Constants.UserPreferredLanguage, me.PreferredLanguage);
                Preferences.Set(Constants.UserSurname, me.Surname);
                Preferences.Set(Constants.UserPrincipalName, me.UserPrincipalName);
                Preferences.Set(Constants.UserID, me.ID);

                await Alerts.ShowAsync(_localization.GetString("Congratulations"), me.DisplayName ?? "null", _localization.GetString("OK"));
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
        ~Login()
        {
            Dispose(disposing: false);
        }

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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}