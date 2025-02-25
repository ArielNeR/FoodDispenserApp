using FoodDispenserApp.Services;
using FoodDispenserApp.ViewModels;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using FoodDispenserApp.Models;

namespace FoodDispenserApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Solo servicios necesarios para MQTT
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddSingleton<ObservableCollection<Horario>>(); // Colección compartida
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<HorariosViewModel>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}