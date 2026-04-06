namespace Hummingbird.Models;

public class AppConfig
{
    public int GlucoseTarget { get; set; } = 120;
    public int CorrectionFactor { get; set; } = 60;
    public int RangeLow { get; set; } = 70;
    public int RangeHigh { get; set; } = 140;
    public int RangeVeryHigh { get; set; } = 250;
    public double InsulinCarbRatio { get; set; } = 10;
}
