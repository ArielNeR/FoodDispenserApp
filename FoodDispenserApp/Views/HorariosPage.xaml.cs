using FoodDispenserApp.ViewModels;

namespace FoodDispenserApp.Views;

public partial class HorariosPage : ContentPage
{
    private readonly HorariosViewModel _viewModel;

    public HorariosPage(HorariosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private async void OnEditHorariosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HorariosEditPage(_viewModel));
    }
}