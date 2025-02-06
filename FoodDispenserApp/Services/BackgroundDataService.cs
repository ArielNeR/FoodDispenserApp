using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using FoodDispenserApp.Services;
using Microsoft.Extensions.Logging;

namespace FoodDispenserApp.Services;

public class BackgroundDataService : BackgroundService
{
    private readonly IApiService _apiService;
    private readonly IMqttService _mqttService;
    private readonly IConnectivityService _connectivityService;
    private readonly ILogger<BackgroundDataService> _logger;
    private Timer? _timer;

    public BackgroundDataService(IApiService apiService,
                                 IMqttService mqttService,
                                 IConnectivityService connectivityService,
                                 ILogger<BackgroundDataService> logger)
    {
        _apiService = apiService;
        _mqttService = mqttService;
        _connectivityService = connectivityService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (await _connectivityService.CheckLocalConnectivityAsync())
        {
            _logger.LogInformation("Modo Local detectado. Se inicia el polling HTTP cada 3 minutos.");
            // Realiza la primera consulta inmediatamente y luego cada 3 minutos.
            _timer = new Timer(async _ => await PollApiAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(3));
        }
        else
        {
            _logger.LogInformation("Modo Remoto detectado. Conectando vía MQTT.");
            await _mqttService.ConnectAsync();
            // En modo remoto se reciben los datos en tiempo real mediante la suscripción.
        }

        // Mantener el servicio activo hasta que se cancele.
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task PollApiAsync()
    {
        try
        {
            var temp = await _apiService.GetTemperatureAsync();
            var hum = await _apiService.GetHumidityAsync();
            var food = await _apiService.GetFoodLevelAsync();
            var horarios = await _apiService.GetHorariosAsync();

            _logger.LogInformation($"Datos API: Temp={temp}, Humedad={hum}, Nivel de comida={food}");

            // Aquí se puede enviar un mensaje o actualizar un estado compartido para que la UI se actualice.
            // Además, se pueden evaluar condiciones para disparar notificaciones (por ejemplo, nivel bajo de comida).
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al hacer polling a la API.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
