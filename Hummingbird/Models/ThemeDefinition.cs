namespace Hummingbird.Models;

public record ThemeDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }

    // Brand colors
    public required string Primary { get; init; }
    public required string PrimaryDark { get; init; }
    public required string PrimaryDarkText { get; init; }
    public required string Secondary { get; init; }
    public required string SecondaryDarkText { get; init; }
    public required string Tertiary { get; init; }
    public required string MidnightBlue { get; init; }

    // Glucose range colors
    public required string GlucoseLow { get; init; }
    public required string GlucoseInRange { get; init; }
    public required string GlucoseHigh { get; init; }
    public required string GlucoseVeryHigh { get; init; }

    // Page
    public required string PageBackground { get; init; }

    public string DisplayName => $"{Icon} {Name}";
}
