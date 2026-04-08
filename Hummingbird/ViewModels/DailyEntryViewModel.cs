using Hummingbird.Models;
using Hummingbird.Services;

namespace Hummingbird.ViewModels;

public class DailyEntryViewModel : BaseViewModel
{
    private readonly DataService _dataService;
    private readonly InsulinCalculatorService _calculatorService;

    public List<string> MeasurementTypesList => MeasurementTypes.All;

    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value); }

    private TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
    public TimeSpan SelectedTime { get => _selectedTime; set => SetProperty(ref _selectedTime, value); }

    private string _selectedMeasurementType = MeasurementTypes.UniqueReading;
    public string SelectedMeasurementType
    {
        get => _selectedMeasurementType;
        set
        {
            if (SetProperty(ref _selectedMeasurementType, value))
            {
                UpdateFormVisibility();
                UpdateCalculation();
            }
        }
    }

    private string _glucoseText = "";
    public string GlucoseText
    {
        get => _glucoseText;
        set
        {
            if (SetProperty(ref _glucoseText, value))
                UpdateCalculation();
        }
    }

    private string _carbohydratesText = "";
    public string CarbohydratesText
    {
        get => _carbohydratesText;
        set
        {
            if (SetProperty(ref _carbohydratesText, value))
                UpdateCalculation();
        }
    }

    private string _activity = "";
    public string Activity { get => _activity; set => SetProperty(ref _activity, value); }

    private string _appliedInsulinText = "";
    public string AppliedInsulinText { get => _appliedInsulinText; set => SetProperty(ref _appliedInsulinText, value); }

    private string _notes = "";
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

    private string _suggestedDose = "0";
    public string SuggestedDose { get => _suggestedDose; set => SetProperty(ref _suggestedDose, value); }

    private string _glucoseStatus = "";
    public string GlucoseStatus { get => _glucoseStatus; set => SetProperty(ref _glucoseStatus, value); }

    private Color _glucoseStatusColor = Colors.Gray;
    public Color GlucoseStatusColor { get => _glucoseStatusColor; set => SetProperty(ref _glucoseStatusColor, value); }

    private string _differenceText = "";
    public string DifferenceText { get => _differenceText; set => SetProperty(ref _differenceText, value); }

    private string _correctionDoseText = "0";
    public string CorrectionDoseText { get => _correctionDoseText; set => SetProperty(ref _correctionDoseText, value); }

    private string _carbDoseText = "0";
    public string CarbDoseText { get => _carbDoseText; set => SetProperty(ref _carbDoseText, value); }

    private string _safeDoseText = "0";
    public string SafeDoseText { get => _safeDoseText; set => SetProperty(ref _safeDoseText, value); }

    private bool _showSafeDose;
    public bool ShowSafeDose { get => _showSafeDose; set => SetProperty(ref _showSafeDose, value); }

    private string _nightTargetText = "";
    public string NightTargetText { get => _nightTargetText; set => SetProperty(ref _nightTargetText, value); }

    private bool _showCarbohydrates;
    public bool ShowCarbohydrates { get => _showCarbohydrates; set => SetProperty(ref _showCarbohydrates, value); }

    private bool _showInsulinCalculator;
    public bool ShowInsulinCalculator { get => _showInsulinCalculator; set => SetProperty(ref _showInsulinCalculator, value); }

    private bool _showExtras;
    public bool ShowExtras { get => _showExtras; set => SetProperty(ref _showExtras, value); }

    private bool _showActivityInput;
    public bool ShowActivityInput { get => _showActivityInput; set => SetProperty(ref _showActivityInput, value); }

    private bool _showNotesInput;
    public bool ShowNotesInput { get => _showNotesInput; set => SetProperty(ref _showNotesInput, value); }

    private AppConfig _config = new();

    public Command SaveCommand { get; }
    public Command ToggleActivityCommand { get; }
    public Command ToggleNotesCommand { get; }

    public DailyEntryViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Nuevo Registro";
        SaveCommand = new Command(async () => await SaveAsync());
        ToggleActivityCommand = new Command(() => ShowActivityInput = !ShowActivityInput);
        ToggleNotesCommand = new Command(() => ShowNotesInput = !ShowNotesInput);
    }

    public async Task InitializeAsync()
    {
        _config = await _dataService.GetConfigAsync();
        SelectedTime = DateTime.Now.TimeOfDay;
        UpdateFormVisibility();
        UpdateCalculation();
    }

    private void UpdateFormVisibility()
    {
        var type = SelectedMeasurementType;
        bool isUnique = type == MeasurementTypes.UniqueReading;
        bool isPreprandial = MeasurementTypes.IsPreprandial(type);

        ShowCarbohydrates = isPreprandial;
        ShowInsulinCalculator = true;
        ShowExtras = true;
        ShowSafeDose = type == MeasurementTypes.PreDinner;
        ShowActivityInput = false;
        ShowNotesInput = false;

        if (!ShowCarbohydrates)
            CarbohydratesText = "";
    }

    private void UpdateCalculation()
    {
        if (int.TryParse(GlucoseText, out var glucose) && glucose > 0)
        {
            int carbs = ShowCarbohydrates && int.TryParse(CarbohydratesText, out var c) ? c : 0;
            var dose = _calculatorService.CalculateDose(glucose, _config, carbs);
            SuggestedDose = $"{dose:F1}";

            var correctionDose = _calculatorService.CalculateCorrectionDose(glucose, _config);
            CorrectionDoseText = $"{correctionDose:F1}";

            var carbDose = _calculatorService.CalculateCarbDose(carbs, _config);
            CarbDoseText = $"{carbDose:F1}";

            if (ShowSafeDose)
            {
                var safeDose = _calculatorService.CalculateSafeDose(glucose, _config, carbs);
                SafeDoseText = $"{safeDose:F1}";
                NightTargetText = $"objetivo nocturno: {_config.NightTarget} mg/dL";
            }

            GlucoseStatus = _calculatorService.GetGlucoseStatus(glucose, _config);
            GlucoseStatusColor = _calculatorService.GetGlucoseColor(glucose, _config);
            var diff = glucose - _config.GlucoseTarget;
            DifferenceText = diff >= 0 ? $"+{diff}" : $"{diff}";
        }
        else
        {
            SuggestedDose = "0";
            CorrectionDoseText = "0";
            CarbDoseText = "0";
            SafeDoseText = "0";
            GlucoseStatus = "";
            GlucoseStatusColor = Colors.Gray;
            DifferenceText = "";
        }
    }

    private async Task SaveAsync()
    {
        if (!int.TryParse(GlucoseText, out var glucose) || glucose <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Error", "Ingresa un valor de glicemia válido.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var reading = new GlucoseReading
            {
                Date = SelectedDate,
                Time = SelectedTime,
                MeasurementType = SelectedMeasurementType,
                Glucose = glucose,
                Carbohydrates = int.TryParse(CarbohydratesText, out var carbs) ? carbs : 0,
                Activity = Activity,
                SuggestedInsulin = double.TryParse(SuggestedDose, out var suggested) ? suggested : 0,
                AppliedInsulin = double.TryParse(AppliedInsulinText, out var applied) ? applied : 0,
                Notes = Notes
            };

            await _dataService.SaveReadingAsync(reading);

            var summary = new List<string>
            {
                $"📅 {reading.FullDateTime:dd/MM/yyyy HH:mm}",
                $"📌 {reading.MeasurementType}",
                $"📊 Glicemia: {glucose} mg/dL ({GlucoseStatus})"
            };

            if (reading.Carbohydrates > 0)
                summary.Add($"🍞 Carbohidratos: {reading.Carbohydrates} g");

            if (reading.SuggestedInsulin > 0)
                summary.Add($"💉 Dosis sugerida: {reading.SuggestedInsulin:F1} u");

            if (reading.AppliedInsulin > 0)
                summary.Add($"💉 Insulina aplicada: {reading.AppliedInsulin:F1} u");

            if (!string.IsNullOrWhiteSpace(reading.Activity))
                summary.Add($"🏃 {reading.Activity}");

            if (!string.IsNullOrWhiteSpace(reading.Notes))
                summary.Add($"📝 {reading.Notes}");

            await Shell.Current.DisplayAlertAsync(
                "Registro guardado ✓",
                string.Join("\n", summary),
                "OK");
            ClearForm();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        SelectedDate = DateTime.Today;
        SelectedTime = DateTime.Now.TimeOfDay;
        SelectedMeasurementType = MeasurementTypes.UniqueReading;
        GlucoseText = "";
        CarbohydratesText = "";
        Activity = "";
        AppliedInsulinText = "";
        Notes = "";
        ShowActivityInput = false;
        ShowNotesInput = false;
    }
}
