using Hummingbird.Services;

namespace Hummingbird
{
    public partial class App : Application
    {
        public App(ThemeService themeService)
        {
            InitializeComponent();
            themeService.ApplyTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
