using FoodDispenserApp.ViewModels;

namespace FoodDispenserApp.Views;

public partial class HorariosPage : ContentPage
{
    public HorariosPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnEditHorariosClicked(object sender, EventArgs e)
    {
        // Navegar a la página de edición de horarios
        await Navigation.PushAsync(new HorariosEditPage((MainViewModel)BindingContext));
    }
}
