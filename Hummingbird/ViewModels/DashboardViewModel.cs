using Hummingbird.Controls;
using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class CalendarDayInfo
{
    public string DayText { get; set; } = "";
    public string AverageText { get; set; } = "";
    public Color BackgroundColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.Transparent;
    public bool HasData { get; set; }
}

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

    private GlucoseChartDrawable? _chartDrawable;
    public GlucoseChartDrawable? ChartDrawable { get => _chartDrawable; set => SetProperty(ref _chartDrawable, value); }

    private bool _hasChartData;
    public bool HasChartData { get => _hasChartData; set => SetProperty(ref _hasChartData, value); }

    private List<CalendarDayInfo> _calendarDays = [];
    public List<CalendarDayInfo> CalendarDays { get => _calendarDays; set => SetProperty(ref _calendarDays, value); }

    private string _calendarMonthText = "";
    public string CalendarMonthText { get => _calendarMonthText; set => SetProperty(ref _calendarMonthText, value); }

    private double _calendarHeight = 240;
    public double CalendarHeight { get => _calendarHeight; set => SetProperty(ref _calendarHeight, value); }

    public Command PreviousMonthCommand { get; }
    public Command NextMonthCommand { get; }

    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private List<GlucoseReading> _allReadings = [];
    private AppConfig _config = new();

    public DashboardViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Dashboard";
        PreviousMonthCommand = new Command(() => { _calendarMonth = _calendarMonth.AddMonths(-1); UpdateCalendar(); });
        NextMonthCommand = new Command(() => { _calendarMonth = _calendarMonth.AddMonths(1); UpdateCalendar(); });
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

            _allReadings = readings;
            _config = config;

            if (last7Days.Count > 0)
            {
                ChartDrawable = new GlucoseChartDrawable
                {
                    Points = last7Days
                        .Select(r => new GlucoseChartDrawable.ChartPoint(r.FullDateTime, r.Glucose))
                        .ToList(),
                    RangeLow = config.RangeLow,
                    RangeHigh = config.RangeHigh,
                    RangeVeryHigh = config.RangeVeryHigh,
                    GlucoseTarget = config.GlucoseTarget
                };
                HasChartData = true;
            }
            else
            {
                ChartDrawable = null;
                HasChartData = false;
            }

            UpdateCalendar();
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

    private void UpdateCalendar()
    {
        var days = new List<CalendarDayInfo>();
        var first = _calendarMonth;
        var daysInMonth = DateTime.DaysInMonth(first.Year, first.Month);
        var startDow = ((int)first.DayOfWeek + 6) % 7;

        for (int i = 0; i < startDow; i++)
            days.Add(new CalendarDayInfo());

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(first.Year, first.Month, day);
            var dayReadings = _allReadings.Where(r => r.Date.Date == date.Date).ToList();
            var isToday = date.Date == DateTime.Today;
            var info = new CalendarDayInfo { DayText = day.ToString() };

            if (dayReadings.Count > 0)
            {
                var avg = (int)dayReadings.Average(r => r.Glucose);
                info.BackgroundColor = _calculatorService.GetGlucoseColor(avg, _config);
                info.TextColor = Colors.White;
                info.AverageText = avg.ToString();
                info.HasData = true;
            }
            else
            {
                info.BackgroundColor = isToday
                    ? Color.FromArgb("#0D9488").WithAlpha(0.15f)
                    : Colors.Transparent;
                info.TextColor = isToday
                    ? Color.FromArgb("#0D9488")
                    : Colors.Gray;
            }

            days.Add(info);
        }

        CalendarDays = days;

        int totalCells = startDow + daysInMonth;
        int rows = (totalCells + 6) / 7;
        CalendarHeight = rows * 47;

        var monthText = first.ToString("MMMM yyyy");
        CalendarMonthText = char.ToUpper(monthText[0]) + monthText[1..];
    }
}
