using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class InsulinCalculatorViewModel : BaseViewModel
{
    private readonly DataService _dataService;

    private string _totalDailyDose = "";
    public string TotalDailyDose
    {
        get => _totalDailyDose;
        set
        {
            if (SetProperty(ref _totalDailyDose, value))
                Recalculate();
        }
    }

    private int _selectedFsiConstantIndex;
    public int SelectedFsiConstantIndex
    {
        get => _selectedFsiConstantIndex;
        set
        {
            if (SetProperty(ref _selectedFsiConstantIndex, value))
            {
                OnPropertyChanged(nameof(FsiConstantLabel));
                Recalculate();
            }
        }
    }

    public List<string> FsiConstantOptions { get; } =
    [
        "1800 — Insulina ultrarrápida",
        "1500 — Insulina rápida",
        "Personalizado"
    ];

    private string _customFsiConstant = "1800";
    public string CustomFsiConstant
    {
        get => _customFsiConstant;
        set
        {
            if (SetProperty(ref _customFsiConstant, value))
                Recalculate();
        }
    }

    public bool IsCustomFsi => SelectedFsiConstantIndex == 2;

    public string FsiConstantLabel => SelectedFsiConstantIndex switch
    {
        0 => "1800",
        1 => "1500",
        _ => CustomFsiConstant
    };

    private string _ratioConstant = "500";
    public string RatioConstant
    {
        get => _ratioConstant;
        set
        {
            if (SetProperty(ref _ratioConstant, value))
                Recalculate();
        }
    }

    private string _fsiResult = "—";
    public string FsiResult { get => _fsiResult; set => SetProperty(ref _fsiResult, value); }

    private string _ratioResult = "—";
    public string RatioResult { get => _ratioResult; set => SetProperty(ref _ratioResult, value); }

    private string _fsiFormula = "";
    public string FsiFormula { get => _fsiFormula; set => SetProperty(ref _fsiFormula, value); }

    private string _ratioFormula = "";
    public string RatioFormula { get => _ratioFormula; set => SetProperty(ref _ratioFormula, value); }

    public Command ApplyToSettingsCommand { get; }

    public InsulinCalculatorViewModel(DataService dataService)
    {
        _dataService = dataService;
        Title = "Calculadora FSI / Ratio";
        ApplyToSettingsCommand = new Command(async () => await ApplyToSettingsAsync(), () => HasValidResults);
    }

    private int GetFsiConstant()
    {
        return SelectedFsiConstantIndex switch
        {
            0 => 1800,
            1 => 1500,
            _ => int.TryParse(CustomFsiConstant, out var c) ? c : 1800
        };
    }

    private void Recalculate()
    {
        OnPropertyChanged(nameof(IsCustomFsi));

        if (!double.TryParse(TotalDailyDose, out var tdd) || tdd <= 0)
        {
            FsiResult = "—";
            RatioResult = "—";
            FsiFormula = "";
            RatioFormula = "";
            ApplyToSettingsCommand.ChangeCanExecute();
            return;
        }

        var fsiConst = GetFsiConstant();
        var ratioConst = int.TryParse(RatioConstant, out var rc) ? rc : 500;

        var fsi = Math.Round(fsiConst / tdd, 1);
        var ratio = Math.Round(ratioConst / tdd, 1);

        FsiResult = fsi.ToString("F1");
        RatioResult = ratio.ToString("F1");
        FsiFormula = $"{fsiConst} / {tdd} = {fsi}";
        RatioFormula = $"{ratioConst} / {tdd} = {ratio}";

        ApplyToSettingsCommand.ChangeCanExecute();
    }

    private bool HasValidResults => FsiResult != "—" && RatioResult != "—";

    private async Task ApplyToSettingsAsync()
    {
        if (!double.TryParse(FsiResult, out var fsi) || !double.TryParse(RatioResult, out var ratio))
            return;

        var config = await _dataService.GetConfigAsync();
        config.CorrectionFactor = (int)Math.Round(fsi);
        config.InsulinCarbRatio = ratio;
        await _dataService.SaveConfigAsync(config);

        await Shell.Current.DisplayAlertAsync(
            "Aplicado",
            $"FSI = {config.CorrectionFactor}, Ratio = {config.InsulinCarbRatio} guardados en configuración.",
            "OK");
    }
}
