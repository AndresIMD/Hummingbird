using Hummingbird.Models;

namespace Hummingbird.Services;

public class InsulinCalculatorService
{
    public double CalculateDose(int glucose, AppConfig config, int carbohydrates = 0)
    {
        var correction = CalculateCorrectionDose(glucose, config);
        var carbs = CalculateCarbDose(carbohydrates, config);
        return Math.Round(correction + carbs, 1);
    }

    public double CalculateCorrectionDose(int glucose, AppConfig config)
    {
        if (glucose <= config.GlucoseTarget)
            return 0;
        return Math.Round((double)(glucose - config.GlucoseTarget) / config.CorrectionFactor, 1);
    }

    public double CalculateCarbDose(int carbohydrates, AppConfig config)
    {
        if (carbohydrates <= 0 || config.InsulinCarbRatio <= 0)
            return 0;
        return Math.Round((double)carbohydrates / config.InsulinCarbRatio, 1);
    }

    public double CalculateSafeDose(int glucose, AppConfig config, int carbohydrates = 0)
    {
        double correctionDose = glucose > config.NightTarget
            ? (double)(glucose - config.NightTarget) / config.CorrectionFactor
            : 0;
        double carbDose = CalculateCarbDose(carbohydrates, config);
        return Math.Round(correctionDose + carbDose, 1);
    }

    public string GetGlucoseStatus(int glucose, AppConfig config)
    {
        if (glucose < config.RangeLow) return "Bajo";
        if (glucose <= config.RangeHigh) return "En rango";
        if (glucose <= config.RangeVeryHigh) return "Alto";
        return "Muy alto";
    }

    public Color GetGlucoseColor(int glucose, AppConfig config)
    {
        if (glucose < config.RangeLow) return Color.FromArgb("#3B82F6");
        if (glucose <= config.RangeHigh) return Color.FromArgb("#10B981");
        if (glucose <= config.RangeVeryHigh) return Color.FromArgb("#F59E0B");
        return Color.FromArgb("#EF4444");
    }
}
