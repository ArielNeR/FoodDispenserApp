using FoodDispenserApp.ViewModels;
using FoodDispenserApp.Views;
using Microcharts;

namespace FoodDispenserApp;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public MainPage(MainViewModel viewModel, IServiceProvider serviceProvider)
    {
        Console.WriteLine("Inicializando MainPage...");
        InitializeComponent();
        BindingContext = viewModel;
        _serviceProvider = serviceProvider;
        Console.WriteLine("MainPage inicializado.");
    }

    private async void OnHorariosClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Navegando a HorariosPage...");
        var horariosViewModel = _serviceProvider.GetRequiredService<HorariosViewModel>();
        await Navigation.PushAsync(new HorariosPage(horariosViewModel));
    }
}