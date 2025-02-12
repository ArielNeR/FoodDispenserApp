using FoodDispenserApp.Services;
using FoodDispenserApp.ViewModels;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

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

            // Comunicación exclusivamente vía MQTT.
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

            // Registra el MainViewModel y la MainPage
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            // (Opcional) Servicio en segundo plano para reintentos o monitoreo
            builder.Services.AddHostedService<BackgroundDataService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
