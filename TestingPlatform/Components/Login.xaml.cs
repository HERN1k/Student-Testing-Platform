using System.Diagnostics;
using System.Globalization;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Components
{
    public partial class Login : ContentView, IDisposable
    {
        private readonly IGraphService _graph;
        private readonly ILocalizationService _localization;
        private bool disposedValue;

        private bool _isPassword { get; set; } = true;

        public Login(IGraphService graph, ILocalizationService localization)
        {
            _graph = graph;
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
            try // //TODO
            {
                await Shell.Current.GoToAsync("Home");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw new NotImplementedException();
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
            try
            {
                await _graph.AuthorizationAsync();
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw new NotImplementedException();
            }

            try //TODO
            {
                await Shell.Current.GoToAsync("Home");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
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