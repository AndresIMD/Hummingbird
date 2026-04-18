using Hummingbird.ViewModels;

namespace Hummingbird.Views;

public partial class InsulinCalculatorPage : ContentPage
{
    public InsulinCalculatorPage(InsulinCalculatorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
