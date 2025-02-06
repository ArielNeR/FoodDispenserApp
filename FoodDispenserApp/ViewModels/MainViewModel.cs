using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using Microcharts;
using SkiaSharp;

namespace FoodDispenserApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IMqttService _mqttService;
        private readonly IConnectivityService _connectivityService;
        private System.Timers.Timer _refreshTimer;

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

        // Datos históricos para gráficos
        private ObservableCollection<ChartEntry> _temperatureHistory = new ObservableCollection<ChartEntry>
{
    new ChartEntry(0)
    {
        Label = "Sin datos",
        ValueLabel = "0",
        Color = SKColor.Parse("#FF0000")
    }
};
        public ObservableCollection<ChartEntry> TemperatureHistory
        {
            get => _temperatureHistory;
            set { _temperatureHistory = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChartEntry> _humidityHistory = new ObservableCollection<ChartEntry>
{
    new ChartEntry(0)
    {
        Label = "Sin datos",
        ValueLabel = "0",
        Color = SKColor.Parse("#0000FF")
    }
};
        public ObservableCollection<ChartEntry> HumidityHistory
        {
            get => _humidityHistory;
            set { _humidityHistory = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChartEntry> _foodLevelHistory = new ObservableCollection<ChartEntry>
{
    new ChartEntry(0)
    {
        Label = "Sin datos",
        ValueLabel = "0",
        Color = SKColor.Parse("#00FF00")
    }
};
        public ObservableCollection<ChartEntry> FoodLevelHistory
        {
            get => _foodLevelHistory;
            set { _foodLevelHistory = value; OnPropertyChanged(); }
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

                // Actualizar datos históricos
                UpdateHistory(data);
            };

            // Establece el estado de conexión
            SetConnectionStatus();

            // Configurar el temporizador para refrescar los datos cada 3 minutos
            _refreshTimer = new System.Timers.Timer(180000); // 180,000 ms = 3 minutos
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        private void UpdateHistory(SensorData data)
        {
            // Agregar nuevos datos a los históricos
            TemperatureHistory.Add(new ChartEntry((float)data.Temperature) { Label = DateTime.Now.ToString("HH:mm"), ValueLabel = data.Temperature.ToString() });
            HumidityHistory.Add(new ChartEntry((float)data.Humidity) { Label = DateTime.Now.ToString("HH:mm"), ValueLabel = data.Humidity.ToString() });
            FoodLevelHistory.Add(new ChartEntry((float)data.FoodLevel) { Label = DateTime.Now.ToString("HH:mm"), ValueLabel = data.FoodLevel.ToString() });

            // Limitar el tamaño de la lista para no acumular demasiados datos
            if (TemperatureHistory.Count > 10) TemperatureHistory.RemoveAt(0);
            if (HumidityHistory.Count > 10) HumidityHistory.RemoveAt(0);
            if (FoodLevelHistory.Count > 10) FoodLevelHistory.RemoveAt(0);
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

                    // Actualizar datos históricos
                    UpdateHistory(new SensorData
                    {
                        Temperature = Temperature,
                        Humidity = Humidity,
                        FoodLevel = FoodLevel,
                        Horarios = Horarios
                    });
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

        private Chart _temperatureChart;
        public Chart TemperatureChart
        {
            get => _temperatureChart;
            set { _temperatureChart = value; OnPropertyChanged(); }
        }

        private Chart _humidityChart;
        public Chart HumidityChart
        {
            get => _humidityChart;
            set { _humidityChart = value; OnPropertyChanged(); }
        }

        private Chart _foodLevelChart;
        public Chart FoodLevelChart
        {
            get => _foodLevelChart;
            set { _foodLevelChart = value; OnPropertyChanged(); }
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
}