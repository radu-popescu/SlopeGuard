using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SlopeGuard
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // no Remote Config calls here
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(new AppShell());
    }
}
