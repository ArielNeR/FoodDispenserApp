using FoodDispenserApp.ViewModels;

namespace FoodDispenserApp.Views;

public partial class HorariosPage : ContentPage
{
    public HorariosPage(HorariosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnEditHorariosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HorariosEditPage((HorariosViewModel)BindingContext));
    }
}