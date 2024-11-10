using TestingPlatform.Components;

namespace TestingPlatform
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalizationMenu _localizationMenu;
        private readonly Login _login;

        public MainPage(LocalizationMenu localizationMenu, Login login)
        {
            ValidateConstructorArguments(localizationMenu, login);
            _localizationMenu = localizationMenu;
            _login = login;
            InitializeComponent();

            MainGrid.SetRow(_login, 1);
            MainGrid.SetColumn(_login, 0);

            MainGrid.Add(_localizationMenu);
            MainGrid.Add(_login);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _localizationMenu.SubscribeToEvents();
            _login.SubscribeToEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _localizationMenu?.Dispose();
            _login?.Dispose();
        }

        private static void ValidateConstructorArguments(LocalizationMenu localizationMenu, Login login)
        {
            ArgumentNullException.ThrowIfNull(localizationMenu, nameof(localizationMenu));
            ArgumentNullException.ThrowIfNull(login, nameof(login));
        }
    }
}