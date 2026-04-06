using Hummingbird.ViewModels;

namespace Hummingbird.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }

    private void OnDeleteTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject obj && obj.BindingContext is ReadingDisplayItem item)
        {
            _viewModel.DeleteCommand.Execute(item);
        }
    }
}
