using SlopeGuard.Services;

namespace SlopeGuard
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Register route for SettingsPage
            Routing.RegisterRoute("mainpage", typeof(MainPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("sessions", typeof(SessionsPage));


            _ = DatabaseService.InitAsync();
        }
    }
}
