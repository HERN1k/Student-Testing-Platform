using TestingPlatform.Components;

namespace TestingPlatform.Pages
{
    public partial class Home : ContentPage
    {
        private readonly LeftSideBar _leftSideBar;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ContentView> _pages = new();

        public Home(LeftSideBar leftSideBar, IServiceProvider serviceProvider)
        {
            ValidateConstructorArguments(leftSideBar, serviceProvider);
            _leftSideBar = leftSideBar;
            _serviceProvider = serviceProvider;
            InitializeComponent();

            HomeGrid.SetRow(_leftSideBar, 0);
            HomeGrid.SetColumn(_leftSideBar, 0);
            HomeGrid.Add(_leftSideBar);

            MappingPages();
            MainThread.InvokeOnMainThreadAsync(() => ChangePage(string.Empty));
        }

        public void MappingPages()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _pages.Add(nameof(HomeView), scope.ServiceProvider.GetRequiredService<HomeView>());
            }
        }

        public void ChangePage(string type)
        {
            ContentView page;
            if (_pages.ContainsKey(type))
            {
                page = _pages.FirstOrDefault(p => p.Key == type).Value;
            }
            else
            {
                page = _pages.FirstOrDefault(p => p.Key == nameof(HomeView)).Value;
            }

            if (page is not null)
            {
                foreach (IView child in HomeGrid.Children)
                {
                    if (HomeGrid.GetRow(child) == 0 && HomeGrid.GetColumn(child) == 2)
                    {
                        HomeGrid.Children.Remove(child);
                        break;
                    }
                }

                HomeGrid.SetRow(page, 0);
                HomeGrid.SetColumn(page, 2);
                HomeGrid.Add(page);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });

            _leftSideBar.SubscribeToEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _leftSideBar?.Dispose();
        }

        private static void ValidateConstructorArguments(LeftSideBar leftSideBar, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(leftSideBar, nameof(leftSideBar));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        }
    }
}