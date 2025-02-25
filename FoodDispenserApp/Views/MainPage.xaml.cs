using FoodDispenserApp.ViewModels;
using FoodDispenserApp.Views;
using Microcharts;

namespace FoodDispenserApp;

public partial class MainPage : ContentPage
{

    private readonly IServiceProvider _serviceProvider;

    public MainPage(MainViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _serviceProvider = serviceProvider;

        // Inicializar los gráficos
        viewModel.TemperatureChart = new LineChart { Entries = viewModel.TemperatureHistory };
        viewModel.HumidityChart = new LineChart { Entries = viewModel.HumidityHistory };
        viewModel.FoodLevelChart = new LineChart { Entries = viewModel.FoodLevelHistory };
    }

    private async void OnHorariosClicked(object sender, EventArgs e)
    {
        var horariosViewModel = _serviceProvider.GetRequiredService<HorariosViewModel>();
        await Navigation.PushAsync(new HorariosPage(horariosViewModel));
    }

}