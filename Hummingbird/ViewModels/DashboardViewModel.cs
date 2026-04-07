using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly DataService _dataService;
    private readonly InsulinCalculatorService _calculatorService;

    private string _averageGlucose = "--";
    public string AverageGlucose { get => _averageGlucose; set => SetProperty(ref _averageGlucose, value); }

    private string _percentInRange = "--";
    public string PercentInRange { get => _percentInRange; set => SetProperty(ref _percentInRange, value); }

    private string _lastGlucose = "--";
    public string LastGlucose { get => _lastGlucose; set => SetProperty(ref _lastGlucose, value); }

    private string _lastGlucoseTime = "";
    public string LastGlucoseTime { get => _lastGlucoseTime; set => SetProperty(ref _lastGlucoseTime, value); }

    private Color _lastGlucoseColor = Colors.Gray;
    public Color LastGlucoseColor { get => _lastGlucoseColor; set => SetProperty(ref _lastGlucoseColor, value); }

    private string _totalInsulin = "--";
    public string TotalInsulin { get => _totalInsulin; set => SetProperty(ref _totalInsulin, value); }

    private string _todaySummary = "0 mediciones hoy";
    public string TodaySummary { get => _todaySummary; set => SetProperty(ref _todaySummary, value); }

    private string _statusMessage = "Cargando...";
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private string _rangeLowText = "Bajo (< 70 mg/dL)";
    public string RangeLowText { get => _rangeLowText; set => SetProperty(ref _rangeLowText, value); }

    private string _rangeInRangeText = "En rango (70 - 140 mg/dL)";
    public string RangeInRangeText { get => _rangeInRangeText; set => SetProperty(ref _rangeInRangeText, value); }

    private string _rangeHighText = "Alto (140 - 250 mg/dL)";
    public string RangeHighText { get => _rangeHighText; set => SetProperty(ref _rangeHighText, value); }

    private string _rangeVeryHighText = "Muy alto (> 250 mg/dL)";
    public string RangeVeryHighText { get => _rangeVeryHighText; set => SetProperty(ref _rangeVeryHighText, value); }

    public DashboardViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Dashboard";
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var readings = await _dataService.GetReadingsAsync();
            var config = await _dataService.GetConfigAsync();
            var now = DateTime.Now;

            RangeLowText = $"Bajo (< {config.RangeLow} mg/dL)";
            RangeInRangeText = $"En rango ({config.RangeLow} - {config.RangeHigh} mg/dL)";
            RangeHighText = $"Alto ({config.RangeHigh} - {config.RangeVeryHigh} mg/dL)";
            RangeVeryHighText = $"Muy alto (> {config.RangeVeryHigh} mg/dL)";
            var last7Days = readings.Where(r => r.FullDateTime >= now.AddDays(-7)).ToList();
            var today = readings.Where(r => r.Date.Date == DateTime.Today).ToList();

            if (last7Days.Count > 0)
            {
                var avg = last7Days.Average(r => r.Glucose);
                AverageGlucose = $"{avg:F0}";
            }
            else
            {
                AverageGlucose = "--";
            }

            if (last7Days.Count > 0)
            {
                var inRange = last7Days.Count(r => r.Glucose >= config.RangeLow && r.Glucose <= config.RangeHigh);
                PercentInRange = $"{(double)inRange / last7Days.Count * 100:F0}%";
            }
            else
            {
                PercentInRange = "--";
            }

            var lastReading = readings.OrderByDescending(r => r.FullDateTime).FirstOrDefault();
            if (lastReading is not null)
            {
                LastGlucose = $"{lastReading.Glucose}";
                LastGlucoseTime = lastReading.FullDateTime.ToString("dd/MM HH:mm");
                LastGlucoseColor = _calculatorService.GetGlucoseColor(lastReading.Glucose, config);
            }
            else
            {
                LastGlucose = "--";
                LastGlucoseTime = "";
                LastGlucoseColor = Colors.Gray;
            }

            if (last7Days.Count > 0)
            {
                var total = last7Days.Sum(r => r.AppliedInsulin);
                TotalInsulin = $"{total:F1} u";
            }
            else
            {
                TotalInsulin = "--";
            }

            TodaySummary = $"{today.Count} mediciones hoy";
            StatusMessage = last7Days.Count == 0
                ? "Sin registros en los últimos 7 días"
                : $"Basado en {last7Days.Count} mediciones (últimos 7 días)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
