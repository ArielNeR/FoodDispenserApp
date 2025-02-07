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
        // Navegar a la p�gina de edici�n de horarios
        await Navigation.PushAsync(new HorariosEditPage((MainViewModel)BindingContext));
    }
}
