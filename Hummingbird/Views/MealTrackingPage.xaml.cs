using Hummingbird.ViewModels;

namespace Hummingbird.Views;

public partial class MealTrackingPage : ContentPage
{
    private readonly MealTrackingViewModel _viewModel;

    public MealTrackingPage(MealTrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }
}
