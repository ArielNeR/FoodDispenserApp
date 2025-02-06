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

            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                // Cambia la URL base según la IP o nombre de host de tu Raspberry Pi
                client.BaseAddress = new Uri("http://192.168.100.82:8000/");
            });

            // Registra los servicios
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

            // Registra el ViewModel y la página principal
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            // Registra el servicio en segundo plano
            builder.Services.AddHostedService<BackgroundDataService>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
