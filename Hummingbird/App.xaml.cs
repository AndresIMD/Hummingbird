using Hummingbird.Services;

namespace Hummingbird
{
    public partial class App : Application
    {
        public App(ThemeService themeService)
        {
            InitializeComponent();
            themeService.ApplyTheme();
            themeService.ApplyAppTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
