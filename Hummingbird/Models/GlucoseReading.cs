using System.Text.Json.Serialization;

namespace Hummingbird.Models;

public class GlucoseReading
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.Today;
    public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay;
    public string MeasurementType { get; set; } = MeasurementTypes.UniqueReading;
    public int Glucose { get; set; }
    public int Carbohydrates { get; set; }
    public string Activity { get; set; } = string.Empty;
    public double SuggestedInsulin { get; set; }
    public double AppliedInsulin { get; set; }
    public string Notes { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime FullDateTime => Date.Date + Time;
}
