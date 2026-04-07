using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly DataService _dataService;
    private readonly ThemeService _themeService;
    private bool _suppressAutoFill;

    public IReadOnlyList<Models.ThemeDefinition> AvailableThemes => _themeService.AvailableThemes;

    private Models.ThemeDefinition _selectedTheme = null!;
    public Models.ThemeDefinition SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value) && value is not null)
            {
                _themeService.ApplyTheme(value);
            }
        }
    }

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                _themeService.SetDarkMode(value);
            }
        }
    }

    private string _slowInsulinDose = "";
    public string SlowInsulinDose
    {
        get => _slowInsulinDose;
        set
        {
            if (SetProperty(ref _slowInsulinDose, value))
                UpdateFSICalculation();
        }
    }

    private string _rapidInsulinDose = "";
    public string RapidInsulinDose
    {
        get => _rapidInsulinDose;
        set
        {
            if (SetProperty(ref _rapidInsulinDose, value))
                UpdateFSICalculation();
        }
    }

    private string _calculatedTDD = "--";
    public string CalculatedTDD { get => _calculatedTDD; set => SetProperty(ref _calculatedTDD, value); }

    private string _calculatedFSI = "--";
    public string CalculatedFSI { get => _calculatedFSI; set => SetProperty(ref _calculatedFSI, value); }

    private string _calculatedRatio = "--";
    public string CalculatedRatio { get => _calculatedRatio; set => SetProperty(ref _calculatedRatio, value); }

    private bool _hasCalculatorValues;
    public bool HasCalculatorValues { get => _hasCalculatorValues; set => SetProperty(ref _hasCalculatorValues, value); }

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

    private string _insulinCarbRatio = "10";
    public string InsulinCarbRatio { get => _insulinCarbRatio; set => SetProperty(ref _insulinCarbRatio, value); }

    private string _nightTarget = "150";
    public string NightTarget { get => _nightTarget; set => SetProperty(ref _nightTarget, value); }

    private string _rangeLow = "70";
    public string RangeLow { get => _rangeLow; set => SetProperty(ref _rangeLow, value); }

    private string _rangeHigh = "140";
    public string RangeHigh { get => _rangeHigh; set => SetProperty(ref _rangeHigh, value); }

    private string _rangeVeryHigh = "250";
    public string RangeVeryHigh { get => _rangeVeryHigh; set => SetProperty(ref _rangeVeryHigh, value); }

    private string _formulaPreview = "(Glicemia - 120) / 60 = Dosis";
    public string FormulaPreview { get => _formulaPreview; set => SetProperty(ref _formulaPreview, value); }

    public Command SaveCommand { get; }

    public SettingsViewModel(DataService dataService, ThemeService themeService)
    {
        _dataService = dataService;
        _themeService = themeService;
        _selectedTheme = _themeService.CurrentTheme;
        _isDarkMode = _themeService.IsDarkMode;
        Title = "Configuración";
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        _suppressAutoFill = true;
        try
        {
            var config = await _dataService.GetConfigAsync();
            GlucoseTarget = config.GlucoseTarget.ToString();
            CorrectionFactor = config.CorrectionFactor.ToString();
            RangeLow = config.RangeLow.ToString();
            RangeHigh = config.RangeHigh.ToString();
            RangeVeryHigh = config.RangeVeryHigh.ToString();
            InsulinCarbRatio = config.InsulinCarbRatio.ToString("F0");
            NightTarget = config.NightTarget.ToString();
            SlowInsulinDose = config.SlowInsulinDose > 0 ? config.SlowInsulinDose.ToString("F0") : "";
            RapidInsulinDose = config.RapidInsulinDose > 0 ? config.RapidInsulinDose.ToString("F0") : "";
        }
        finally
        {
            _suppressAutoFill = false;
        }
    }

    private void UpdateFormulaPreview()
    {
        FormulaPreview = $"(Glicemia - {GlucoseTarget}) / {CorrectionFactor} = Dosis";
    }

    private void UpdateFSICalculation()
    {
        if (double.TryParse(SlowInsulinDose, out var slow) && slow > 0 &&
            double.TryParse(RapidInsulinDose, out var rapid) && rapid >= 0)
        {
            var tdd = slow + rapid;
            var fsi = 1800.0 / tdd;
            var ratio = 450.0 / tdd;

            CalculatedTDD = $"{tdd:F0} u";
            CalculatedFSI = $"{fsi:F0}";
            CalculatedRatio = $"{ratio:F1}";
            HasCalculatorValues = true;

            if (!_suppressAutoFill)
            {
                CorrectionFactor = CalculatedFSI;
                InsulinCarbRatio = CalculatedRatio;
            }
        }
        else
        {
            CalculatedTDD = "--";
            CalculatedFSI = "--";
            CalculatedRatio = "--";
            HasCalculatorValues = false;
        }
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
            InsulinCarbRatio = double.TryParse(InsulinCarbRatio, out var ratio) ? ratio : 10,
            NightTarget = int.TryParse(NightTarget, out var nightTarget) ? nightTarget : 150,
            SlowInsulinDose = double.TryParse(SlowInsulinDose, out var slow) ? slow : 0,
            RapidInsulinDose = double.TryParse(RapidInsulinDose, out var rapid) ? rapid : 0
        };

        await _dataService.SaveConfigAsync(config);
        await Shell.Current.DisplayAlertAsync("Guardado", "Configuración actualizada correctamente.", "OK");
    }
}
