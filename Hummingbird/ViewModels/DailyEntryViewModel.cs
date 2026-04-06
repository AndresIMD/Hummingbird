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
    public string SelectedMeasurementType { get => _selectedMeasurementType; set => SetProperty(ref _selectedMeasurementType, value); }

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
    public string CarbohydratesText { get => _carbohydratesText; set => SetProperty(ref _carbohydratesText, value); }

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

    private AppConfig _config = new();

    public Command SaveCommand { get; }

    public DailyEntryViewModel(DataService dataService, InsulinCalculatorService calculatorService)
    {
        _dataService = dataService;
        _calculatorService = calculatorService;
        Title = "Nuevo Registro";
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task InitializeAsync()
    {
        _config = await _dataService.GetConfigAsync();
        SelectedTime = DateTime.Now.TimeOfDay;
        UpdateCalculation();
    }

    private void UpdateCalculation()
    {
        if (int.TryParse(GlucoseText, out var glucose) && glucose > 0)
        {
            var dose = _calculatorService.CalculateDose(glucose, _config);
            SuggestedDose = $"{dose:F1}";
            GlucoseStatus = _calculatorService.GetGlucoseStatus(glucose, _config);
            GlucoseStatusColor = _calculatorService.GetGlucoseColor(glucose, _config);
            var diff = glucose - _config.GlucoseTarget;
            DifferenceText = diff >= 0 ? $"+{diff}" : $"{diff}";
        }
        else
        {
            SuggestedDose = "0";
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
            await Shell.Current.DisplayAlertAsync(
                "Guardado",
                $"Glicemia: {glucose} mg/dL\nDosis sugerida: {SuggestedDose} u",
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
    }
}
