using TestingPlatform.Pages;

namespace TestingPlatform
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("Home", typeof(Home));
        }
    }
}
