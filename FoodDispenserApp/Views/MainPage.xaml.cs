using FoodDispenserApp.ViewModels;
using FoodDispenserApp.Views;
using Microcharts;

namespace FoodDispenserApp;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Inicializar los gráficos
        viewModel.TemperaturaChart = new Microcharts.LineChart { Entries = viewModel.TemperaturaHistory };
        viewModel.HumedadChart = new Microcharts.LineChart { Entries = viewModel.HumedadHistory };
        viewModel.UltrasonidoChart = new Microcharts.LineChart { Entries = viewModel.UltrasonidoHistory };
    }

    private async void OnHorariosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HorariosPage((MainViewModel)BindingContext));
    }
}
