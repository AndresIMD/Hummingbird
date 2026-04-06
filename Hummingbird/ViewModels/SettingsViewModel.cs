using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly DataService _dataService;

    private string _glucoseTarget = "120";
    public string GlucoseTarget
    {
        get => _glucoseTarget;
        set
        {
            if (SetProperty(ref _glucoseTarget, value))
                UpdateFormulaPreview();
        }
    }

    private string _correctionFactor = "60";
    public string CorrectionFactor
    {
        get => _correctionFactor;
        set
        {
            if (SetProperty(ref _correctionFactor, value))
                UpdateFormulaPreview();
        }
    }

    private string _rangeLow = "70";
    public string RangeLow { get => _rangeLow; set => SetProperty(ref _rangeLow, value); }

    private string _rangeHigh = "140";
    public string RangeHigh { get => _rangeHigh; set => SetProperty(ref _rangeHigh, value); }

    private string _rangeVeryHigh = "250";
    public string RangeVeryHigh { get => _rangeVeryHigh; set => SetProperty(ref _rangeVeryHigh, value); }

    private string _insulinCarbRatio = "10";
    public string InsulinCarbRatio { get => _insulinCarbRatio; set => SetProperty(ref _insulinCarbRatio, value); }

    private string _formulaPreview = "(Glicemia - 120) / 60 = Dosis";
    public string FormulaPreview { get => _formulaPreview; set => SetProperty(ref _formulaPreview, value); }

    public Command SaveCommand { get; }

    public SettingsViewModel(DataService dataService)
    {
        _dataService = dataService;
        Title = "Configuración";
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        var config = await _dataService.GetConfigAsync();
        GlucoseTarget = config.GlucoseTarget.ToString();
        CorrectionFactor = config.CorrectionFactor.ToString();
        RangeLow = config.RangeLow.ToString();
        RangeHigh = config.RangeHigh.ToString();
        RangeVeryHigh = config.RangeVeryHigh.ToString();
        InsulinCarbRatio = config.InsulinCarbRatio.ToString("F0");
    }

    private void UpdateFormulaPreview()
    {
        FormulaPreview = $"(Glicemia - {GlucoseTarget}) / {CorrectionFactor} = Dosis";
    }

    private async Task SaveAsync()
    {
        var config = new AppConfig
        {
            GlucoseTarget = int.TryParse(GlucoseTarget, out var target) ? target : 120,
            CorrectionFactor = int.TryParse(CorrectionFactor, out var factor) ? factor : 60,
            RangeLow = int.TryParse(RangeLow, out var low) ? low : 70,
            RangeHigh = int.TryParse(RangeHigh, out var high) ? high : 140,
            RangeVeryHigh = int.TryParse(RangeVeryHigh, out var veryHigh) ? veryHigh : 250,
            InsulinCarbRatio = double.TryParse(InsulinCarbRatio, out var ratio) ? ratio : 10
        };

        await _dataService.SaveConfigAsync(config);
        await Shell.Current.DisplayAlertAsync("Guardado", "Configuración actualizada correctamente.", "OK");
    }
}
