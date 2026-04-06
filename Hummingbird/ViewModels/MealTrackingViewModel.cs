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

    public MealTrackingViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Seguimiento";
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
        }
        finally
        {
            IsBusy = false;
        }
    }
}
