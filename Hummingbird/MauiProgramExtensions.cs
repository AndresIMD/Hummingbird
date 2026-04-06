using Microsoft.Extensions.Logging;
using Hummingbird.Services;
using Hummingbird.ViewModels;
using Hummingbird.Views;

namespace Hummingbird
{
    public static class MauiProgramExtensions
    {
        public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddSingleton<InsulinCalculatorService>();

            // ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DailyEntryViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<MealTrackingViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<DailyEntryPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<MealTrackingPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder;
        }
    }
}
