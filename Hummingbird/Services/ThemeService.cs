using Hummingbird.Models;

namespace Hummingbird.Services;

public class ThemeService
{
    private const string ThemePreferenceKey = "selected_theme_id";
    private const string DarkModePreferenceKey = "is_dark_mode";
    private const string DefaultThemeId = "teal";

    private readonly List<ThemeDefinition> _themes =
    [
        new ThemeDefinition
        {
            Id = "teal",
            Name = "Teal",
            Icon = "🌿",
            Primary = "#0D9488",
            PrimaryDark = "#5EEAD4",
            PrimaryDarkText = "#1A1A1A",
            Secondary = "#CCF4EE",
            SecondaryDarkText = "#5EEAD4",
            Tertiary = "#065F53",
            MidnightBlue = "#064E3B",
            GlucoseLow = "#3B82F6",
            GlucoseInRange = "#10B981",
            GlucoseHigh = "#F59E0B",
            GlucoseVeryHigh = "#EF4444",
            PageBackground = "#F0F4F8"
        },
        new ThemeDefinition
        {
            Id = "ocean",
            Name = "Océano",
            Icon = "🌊",
            Primary = "#2563EB",
            PrimaryDark = "#60A5FA",
            PrimaryDarkText = "#1A1A1A",
            Secondary = "#DBEAFE",
            SecondaryDarkText = "#60A5FA",
            Tertiary = "#1E3A8A",
            MidnightBlue = "#1E3A5F",
            GlucoseLow = "#3B82F6",
            GlucoseInRange = "#10B981",
            GlucoseHigh = "#F59E0B",
            GlucoseVeryHigh = "#EF4444",
            PageBackground = "#EFF6FF"
        },
        new ThemeDefinition
        {
            Id = "rose",
            Name = "Rosa",
            Icon = "🌸",
            Primary = "#E11D48",
            PrimaryDark = "#FB7185",
            PrimaryDarkText = "#1A1A1A",
            Secondary = "#FFE4E6",
            SecondaryDarkText = "#FB7185",
            Tertiary = "#9F1239",
            MidnightBlue = "#881337",
            GlucoseLow = "#3B82F6",
            GlucoseInRange = "#10B981",
            GlucoseHigh = "#F59E0B",
            GlucoseVeryHigh = "#EF4444",
            PageBackground = "#FFF1F2"
        }
    ];

    public IReadOnlyList<ThemeDefinition> AvailableThemes => _themes;

    public ThemeDefinition CurrentTheme { get; private set; }

    public bool IsDarkMode { get; private set; }

    public ThemeService()
    {
        var savedId = Preferences.Get(ThemePreferenceKey, DefaultThemeId);
        CurrentTheme = _themes.Find(t => t.Id == savedId) ?? _themes[0];
        IsDarkMode = Preferences.Get(DarkModePreferenceKey, false);
    }

    public void ApplyTheme(ThemeDefinition? theme = null)
    {
        theme ??= CurrentTheme;
        CurrentTheme = theme;
        Preferences.Set(ThemePreferenceKey, theme.Id);

        var resources = Application.Current?.Resources;
        if (resources is null) return;

        SetColor(resources, "Primary", theme.Primary);
        SetColor(resources, "PrimaryDark", theme.PrimaryDark);
        SetColor(resources, "PrimaryDarkText", theme.PrimaryDarkText);
        SetColor(resources, "Secondary", theme.Secondary);
        SetColor(resources, "SecondaryDarkText", theme.SecondaryDarkText);
        SetColor(resources, "Tertiary", theme.Tertiary);
        SetColor(resources, "MidnightBlue", theme.MidnightBlue);
        SetColor(resources, "GlucoseLow", theme.GlucoseLow);
        SetColor(resources, "GlucoseInRange", theme.GlucoseInRange);
        SetColor(resources, "GlucoseHigh", theme.GlucoseHigh);
        SetColor(resources, "GlucoseVeryHigh", theme.GlucoseVeryHigh);
        SetColor(resources, "PageBackground", theme.PageBackground);

        SetBrush(resources, "PrimaryBrush", theme.Primary);
        SetBrush(resources, "SecondaryBrush", theme.Secondary);
        SetBrush(resources, "TertiaryBrush", theme.Tertiary);
    }

    public void SetDarkMode(bool isDark)
    {
        IsDarkMode = isDark;
        Preferences.Set(DarkModePreferenceKey, isDark);
        ApplyAppTheme();
    }

    public void ApplyAppTheme()
    {
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private static void SetColor(ResourceDictionary resources, string key, string hex)
    {
        var color = Color.FromArgb(hex);

        foreach (var dict in resources.MergedDictionaries)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = color;
                return;
            }
        }

        resources[key] = color;
    }

    private static void SetBrush(ResourceDictionary resources, string key, string hex)
    {
        var brush = new SolidColorBrush(Color.FromArgb(hex));

        foreach (var dict in resources.MergedDictionaries)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = brush;
                return;
            }
        }

        resources[key] = brush;
    }
}
