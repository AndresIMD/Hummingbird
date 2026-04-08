using System.Collections.ObjectModel;
using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class MealPairSummary : BaseViewModel
{
    public string MealName { get; set; } = "";
    public string MealEmoji { get; set; } = "";

    private double _preAverage;
    public double PreAverage { get => _preAverage; set => SetProperty(ref _preAverage, value); }

    private double _postAverage;
    public double PostAverage { get => _postAverage; set => SetProperty(ref _postAverage, value); }

    private int _preCount;
    public int PreCount { get => _preCount; set => SetProperty(ref _preCount, value); }

    private int _postCount;
    public int PostCount { get => _postCount; set => SetProperty(ref _postCount, value); }

    private Color _preColor = Colors.Gray;
    public Color PreColor { get => _preColor; set => SetProperty(ref _preColor, value); }

    private Color _postColor = Colors.Gray;
    public Color PostColor { get => _postColor; set => SetProperty(ref _postColor, value); }

    private string _deltaText = "--";
    public string DeltaText { get => _deltaText; set => SetProperty(ref _deltaText, value); }
}

public class MealDayDetail
{
    public string DayLabel { get; set; } = "";
    public string PreGlucose { get; set; } = "--";
    public string PostGlucose { get; set; } = "--";
    public Color PreColor { get; set; } = Colors.Gray;
    public Color PostColor { get; set; } = Colors.Gray;
    public string DeltaText { get; set; } = "";
}

public class MealWeekSection
{
    public string MealName { get; set; } = "";
    public string MealEmoji { get; set; } = "";
    public List<MealDayDetail> Days { get; set; } = [];
    public bool NoData { get; set; } = true;
    public bool HasData { get; set; }
}

public class MealTrackingViewModel : BaseViewModel
{
    private readonly DataService _dataService;
    private readonly InsulinCalculatorService _calculatorService;

    public ObservableCollection<MealPairSummary> MealSummaries { get; } = [];

    public List<string> Periods { get; } = ["Hoy", "Última semana", "Último mes", "Todo"];

    private string _selectedPeriod = "Última semana";
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod, value))
                _ = LoadDataAsync();
        }
    }

    private string _periodSummary = "";
    public string PeriodSummary { get => _periodSummary; set => SetProperty(ref _periodSummary, value); }

    public ObservableCollection<MealWeekSection> WeekMealDetails { get; } = [];

    private DateTime _weekStart;

    private string _weekLabel = "";
    public string WeekLabel { get => _weekLabel; set => SetProperty(ref _weekLabel, value); }

    public Command PreviousWeekCommand { get; }
    public Command NextWeekCommand { get; }

    public MealTrackingViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Seguimiento";
        _weekStart = GetMonday(DateTime.Today);
        PreviousWeekCommand = new Command(() => { _weekStart = _weekStart.AddDays(-7); _ = LoadDataAsync(); });
        NextWeekCommand = new Command(() => { _weekStart = _weekStart.AddDays(7); _ = LoadDataAsync(); });
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var allReadings = await _dataService.GetReadingsAsync();
            var config = await _dataService.GetConfigAsync();
            var now = DateTime.Now;

            var filtered = SelectedPeriod switch
            {
                "Hoy" => allReadings.Where(r => r.Date.Date == DateTime.Today).ToList(),
                "Última semana" => allReadings.Where(r => r.FullDateTime >= now.AddDays(-7)).ToList(),
                "Último mes" => allReadings.Where(r => r.FullDateTime >= now.AddMonths(-1)).ToList(),
                _ => allReadings.ToList()
            };

            var meals = new[]
            {
                ("Desayuno", "🌅", MeasurementTypes.PreBreakfast, MeasurementTypes.PostBreakfast),
                ("Almuerzo", "☀️", MeasurementTypes.PreLunch, MeasurementTypes.PostLunch),
                ("Cena", "🌙", MeasurementTypes.PreDinner, MeasurementTypes.PostDinner)
            };

            MealSummaries.Clear();
            foreach (var (name, emoji, preType, postType) in meals)
            {
                var preReadings = filtered.Where(r => r.MeasurementType == preType).ToList();
                var postReadings = filtered.Where(r => r.MeasurementType == postType).ToList();

                var preAvg = preReadings.Count > 0 ? preReadings.Average(r => r.Glucose) : 0;
                var postAvg = postReadings.Count > 0 ? postReadings.Average(r => r.Glucose) : 0;
                var delta = (preReadings.Count > 0 && postReadings.Count > 0) ? postAvg - preAvg : 0;

                MealSummaries.Add(new MealPairSummary
                {
                    MealName = name,
                    MealEmoji = emoji,
                    PreAverage = preAvg,
                    PostAverage = postAvg,
                    PreCount = preReadings.Count,
                    PostCount = postReadings.Count,
                    PreColor = preReadings.Count > 0 ? _calculatorService.GetGlucoseColor((int)preAvg, config) : Colors.Gray,
                    PostColor = postReadings.Count > 0 ? _calculatorService.GetGlucoseColor((int)postAvg, config) : Colors.Gray,
                    DeltaText = delta != 0 ? (delta > 0 ? $"+{delta:F0}" : $"{delta:F0}") : "--"
                });
            }

            var totalPre = filtered.Count(r => MeasurementTypes.IsPreprandial(r.MeasurementType));
            var totalPost = filtered.Count(r => MeasurementTypes.IsPostprandial(r.MeasurementType));
            PeriodSummary = $"{totalPre} preprandiales · {totalPost} postprandiales";

            LoadWeekDetails(allReadings, config);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static DateTime GetMonday(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private void LoadWeekDetails(IEnumerable<GlucoseReading> allReadings, AppConfig config)
    {
        var weekEnd = _weekStart.AddDays(6);
        WeekLabel = $"{_weekStart:dd MMM} – {weekEnd:dd MMM yyyy}";

        var weekReadings = allReadings
            .Where(r => r.Date.Date >= _weekStart && r.Date.Date <= weekEnd)
            .ToList();

        var meals = new[]
        {
            ("Desayuno", "🌅", MeasurementTypes.PreBreakfast, MeasurementTypes.PostBreakfast),
            ("Almuerzo", "☀️", MeasurementTypes.PreLunch, MeasurementTypes.PostLunch),
            ("Cena", "🌙", MeasurementTypes.PreDinner, MeasurementTypes.PostDinner)
        };

        WeekMealDetails.Clear();
        foreach (var (name, emoji, preType, postType) in meals)
        {
            var section = new MealWeekSection { MealName = name, MealEmoji = emoji };
            var days = new List<MealDayDetail>();
            bool hasAny = false;

            for (int i = 0; i < 7; i++)
            {
                var date = _weekStart.AddDays(i);
                var preReading = weekReadings
                    .Where(r => r.Date.Date == date && r.MeasurementType == preType)
                    .OrderByDescending(r => r.Time)
                    .FirstOrDefault();
                var postReading = weekReadings
                    .Where(r => r.Date.Date == date && r.MeasurementType == postType)
                    .OrderByDescending(r => r.Time)
                    .FirstOrDefault();

                if (preReading is null && postReading is null)
                    continue;

                hasAny = true;
                int? preGlucose = preReading?.Glucose;
                int? postGlucose = postReading?.Glucose;
                int? delta = (preGlucose.HasValue && postGlucose.HasValue)
                    ? postGlucose.Value - preGlucose.Value
                    : null;

                days.Add(new MealDayDetail
                {
                    DayLabel = date.ToString("ddd dd"),
                    PreGlucose = preGlucose?.ToString() ?? "--",
                    PostGlucose = postGlucose?.ToString() ?? "--",
                    PreColor = preGlucose.HasValue
                        ? _calculatorService.GetGlucoseColor(preGlucose.Value, config)
                        : Colors.Gray,
                    PostColor = postGlucose.HasValue
                        ? _calculatorService.GetGlucoseColor(postGlucose.Value, config)
                        : Colors.Gray,
                    DeltaText = delta.HasValue
                        ? (delta.Value >= 0 ? $"+{delta.Value}" : $"{delta.Value}")
                        : ""
                });
            }

            section.Days = days;
            section.NoData = !hasAny;
            section.HasData = hasAny;
            WeekMealDetails.Add(section);
        }
    }
}
