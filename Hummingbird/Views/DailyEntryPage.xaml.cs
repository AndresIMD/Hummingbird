using Hummingbird.ViewModels;

namespace Hummingbird.Views;

public partial class DailyEntryPage : ContentPage
{
    private readonly DailyEntryViewModel _viewModel;

    public DailyEntryPage(DailyEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
