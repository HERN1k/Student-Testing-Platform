using System.Globalization;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Pages;
using TestingPlatform.Utilities;

namespace TestingPlatform.Components
{
    public partial class LeftSideBar : ContentView, IDisposable
    {
        private readonly ILocalizationService _localization;

        private HashSet<string> _buttons { get; } = new()
        {
            "HomeButton",
            "StarButton",
            "UsersButton",
            "SettingsButton"
        };
        private string _currentButton { get; set; } = "HomeButton";
        private Home? _home { get; set; }

        private bool disposedValue;

        public LeftSideBar(ILocalizationService localization)
        {
            _localization = localization;

            InitializeComponent();

            ChangeCurrentButton(string.Empty);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            _home = this.Parent.Parent as Home ?? throw new ArgumentNullException(nameof(Home));
        }

        public void SubscribeToEvents()
        {
            _localization.CultureChanged += OnCultureChanged;
            GitHubButton.Clicked += GitHubButtonClicked;
        }

        private void ChangeCurrentButton(string newCurrentButton)
        {
            if (string.IsNullOrEmpty(newCurrentButton))
                _currentButton = "HomeButton";
            else
                _currentButton = newCurrentButton;

            this.FindByName<ImageButton>(_currentButton).BackgroundColor = Color.FromArgb("#182c45");

            List<string> normalButtons = _buttons
                .Where(e => e != _currentButton)
                .ToList();

            if (normalButtons is null)
                return;

            foreach (string button in normalButtons)
            {
                this.FindByName<ImageButton>(button).BackgroundColor = new Color(0, 0, 0, 0);
            }
        }

        private void OnCultureChanged(object? sender, CultureInfo newCulture)
        {
            UpdateTexts(newCulture);
        }

        private async void OnPointerEntered(object? sender, EventArgs e) =>
            await SideBarBorder.WidthTo(180.0D, 250, Easing.CubicIn);

        private async void OnPointerExited(object? sender, EventArgs e) =>
            await SideBarBorder.WidthTo(65.0D, 250, Easing.CubicIn);

        private void OnPointerEnteredLabel(object? sender, EventArgs e)
        {
            Label? label = sender as Label;
            if (label?.Parent is HorizontalStackLayout stack)
            {
                ImageButton? imageButton = stack.FindByName<ImageButton>(label.AutomationId);
                if (imageButton != null)
                {
                    VisualStateManager.GoToState(imageButton, "PointerOver");
                }
            }
        }

        private void OnPointerExitedLabel(object? sender, EventArgs e)
        {
            Label? label = sender as Label;
            if (label?.Parent is HorizontalStackLayout stack)
            {
                ImageButton? imageButton = stack.FindByName<ImageButton>(label.AutomationId);
                if (imageButton != null)
                {
                    VisualStateManager.GoToState(imageButton, "Normal");
                }
            }
        }

        private void OnPointerPressedLabel(object? sender, EventArgs e)
        {
            Label? label = sender as Label;
            if (label?.Parent is HorizontalStackLayout stack)
            {
                ImageButton? imageButton = stack.FindByName<ImageButton>(label.AutomationId);
                if (imageButton != null)
                {
                    VisualStateManager.GoToState(imageButton, "Pressed");
                    label.Scale = 0.95;
                    label.Opacity = 0.95;
                    label.TranslationX = -1;
                }
            }
        }

        private void OnPointerReleasedLabel(object? sender, EventArgs e)
        {
            Label? label = sender as Label;
            if (label?.Parent is HorizontalStackLayout stack)
            {
                ImageButton? imageButton = stack.FindByName<ImageButton>(label.AutomationId);
                if (imageButton != null)
                {
                    VisualStateManager.GoToState(imageButton, "Normal");
                    label.Scale = 1;
                    label.Opacity = 1;
                    label.TranslationX = +1;
                    ChangeCurrentButton(label.AutomationId);
                    _home?.ChangePage(imageButton.CommandParameter.ToString() ?? string.Empty);
                }
            }
        }

        private void MenuButtonClicked(object? sender, EventArgs e)
        {
            ImageButton? imageButton = sender as ImageButton;
            if (imageButton?.Parent is HorizontalStackLayout stack)
            {
                if (stack.Children.FirstOrDefault(e => e.GetType() == typeof(Label)) is Label label)
                {
                    ChangeCurrentButton(label.AutomationId);
                    _home?.ChangePage(imageButton.CommandParameter.ToString() ?? string.Empty);
                }
            }
        }

        private async void GitHubButtonClicked(object? sender, EventArgs e) =>
            await Launcher.Default.OpenAsync(new Uri(Constants.GitHubUrl));

        private void UpdateTexts(CultureInfo culture)
        {
            HomeButtonLabel.Text = _localization.GetString("Home", culture);
            StarButtonLabel.Text = _localization.GetString("Assessments", culture);
            UsersButtonLabel.Text = _localization.GetString("Students", culture);
            SettingsButtonLabel.Text = _localization.GetString("Settings", culture);
        }

        #region Disposing
        ~LeftSideBar()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _localization.CultureChanged -= OnCultureChanged;

                    if (GitHubButton is not null)
                        GitHubButton.Clicked -= GitHubButtonClicked;
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