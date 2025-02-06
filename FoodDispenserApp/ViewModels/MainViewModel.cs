using System.ComponentModel;
using System.Runtime.CompilerServices;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using System.Windows.Input;

namespace FoodDispenserApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IApiService _apiService;
    private readonly IMqttService _mqttService;
    private readonly IConnectivityService _connectivityService;

    private double _temperature;
    public double Temperature
    {
        get => _temperature;
        set { _temperature = value; OnPropertyChanged(); }
    }

    private double _humidity;
    public double Humidity
    {
        get => _humidity;
        set { _humidity = value; OnPropertyChanged(); }
    }

    private double _foodLevel;
    public double FoodLevel
    {
        get => _foodLevel;
        set { _foodLevel = value; OnPropertyChanged(); }
    }

    private List<Horario> _horarios = new();
    public List<Horario> Horarios
    {
        get => _horarios;
        set { _horarios = value; OnPropertyChanged(); }
    }

    private string _connectionStatus = "Desconocido";
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set { _connectionStatus = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ActivateMotorCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(IApiService apiService,
                         IMqttService mqttService,
                         IConnectivityService connectivityService)
    {
        _apiService = apiService;
        _mqttService = mqttService;
        _connectivityService = connectivityService;

        RefreshCommand = new Command(async () => await RefreshDataAsync());
        ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());

        // Se suscribe al evento de datos provenientes de MQTT
        _mqttService.OnSensorDataReceived += (s, data) =>
        {
            Temperature = data.Temperature;
            Humidity = data.Humidity;
            FoodLevel = data.FoodLevel;
            Horarios = data.Horarios;
        };

        // Establece el estado de conexión
        SetConnectionStatus();
    }

    private void SetConnectionStatus()
    {
        if (_connectivityService.IsLocal)
            ConnectionStatus = "Conectado en modo Local (HTTP)";
        else
            ConnectionStatus = "Conectado en modo Remoto (MQTT)";
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            if (_connectivityService.IsLocal)
            {
                Temperature = await _apiService.GetTemperatureAsync();
                Humidity = await _apiService.GetHumidityAsync();
                FoodLevel = await _apiService.GetFoodLevelAsync();
                Horarios = await _apiService.GetHorariosAsync();
            }
            // En modo remoto, la actualización se produce vía MQTT
        }
        catch (Exception)
        {
            ConnectionStatus = "Error en la conexión";
            // Aquí se puede disparar una notificación de error.
        }

        // Ejemplo de notificación interna si el nivel de comida es muy bajo:
        if (FoodLevel < 10)
        {
            // Aquí se podría usar DisplayAlert o un sistema de notificaciones.
            // Ejemplo: Application.Current?.MainPage?.DisplayAlert("Alerta", "Nivel de comida bajo", "OK");
        }
    }

    public async Task ActivateMotorAsync()
    {
        try
        {
            if (_connectivityService.IsLocal)
                await _apiService.ActivateMotorAsync();
            else
                await _mqttService.PublishActivateMotorAsync();
        }
        catch (Exception)
        {
            // Manejo de error en la activación del motor.
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
