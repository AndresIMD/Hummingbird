using System.Collections.ObjectModel;
using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class ReadingDisplayItem
{
    public GlucoseReading Reading { get; }
    public Color GlucoseColor { get; }
    public string GlucoseStatus { get; }
    public string TimeDisplay { get; }
    public string Summary { get; }

    public ReadingDisplayItem(GlucoseReading reading, InsulinCalculatorService calc, AppConfig config)
    {
        Reading = reading;
        GlucoseColor = calc.GetGlucoseColor(reading.Glucose, config);
        GlucoseStatus = calc.GetGlucoseStatus(reading.Glucose, config);
        TimeDisplay = reading.FullDateTime.ToString("HH:mm");
        var insulinInfo = reading.AppliedInsulin > 0 ? $"{reading.AppliedInsulin:F1}u insulina" : "Sin insulina";
        Summary = $"{reading.MeasurementType} · {insulinInfo}";
    }
}

public class ReadingGroup : ObservableCollection<ReadingDisplayItem>
{
    public string DateDisplay { get; }

    public ReadingGroup(string dateDisplay, IEnumerable<ReadingDisplayItem> items) : base(items)
    {
        DateDisplay = dateDisplay;
    }
}

public class HistoryViewModel : BaseViewModel
{
    private readonly DataService _dataService;
    private readonly InsulinCalculatorService _calculatorService;

    public ObservableCollection<ReadingGroup> GroupedReadings { get; } = [];

    private string _summaryText = "";
    public string SummaryText { get => _summaryText; set => SetProperty(ref _summaryText, value); }

    public Command<ReadingDisplayItem> DeleteCommand { get; }

    public HistoryViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Historial";
        DeleteCommand = new Command<ReadingDisplayItem>(async (item) => await DeleteAsync(item));
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var readings = await _dataService.GetReadingsAsync();
            var config = await _dataService.GetConfigAsync();

            var groups = readings
                .OrderByDescending(r => r.FullDateTime)
                .GroupBy(r => r.Date.Date)
                .Select(g => new ReadingGroup(
                    g.Key.ToString("dddd, dd MMMM yyyy"),
                    g.Select(r => new ReadingDisplayItem(r, _calculatorService, config))))
                .ToList();

            GroupedReadings.Clear();
            foreach (var group in groups)
                GroupedReadings.Add(group);

            SummaryText = $"{readings.Count} registros en total";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync(ReadingDisplayItem item)
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Eliminar", "¿Eliminar este registro?", "Sí", "No");
        if (!confirm) return;

        await _dataService.DeleteReadingAsync(item.Reading.Id);
        await LoadDataAsync();
    }
}
