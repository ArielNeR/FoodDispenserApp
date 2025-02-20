﻿using FoodDispenserApp.ViewModels;
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
        viewModel.TemperatureChart = new LineChart { Entries = viewModel.TemperatureHistory };
        viewModel.HumidityChart = new LineChart { Entries = viewModel.HumidityHistory };
        viewModel.FoodLevelChart = new LineChart { Entries = viewModel.FoodLevelHistory };
    }

    private async void OnHorariosClicked(object sender, EventArgs e)
    {
        // Navegar a la página de Horarios
        await Navigation.PushAsync(new HorariosPage((MainViewModel)BindingContext));
    }

}