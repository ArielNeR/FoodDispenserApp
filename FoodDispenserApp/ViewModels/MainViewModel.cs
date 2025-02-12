using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using Microcharts;
using SkiaSharp;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

namespace FoodDispenserApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
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

        private string _connectionStatus = "Esperando actualización...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        // Historial de datos para gráficos, inicializados con un valor predeterminado.
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

        // Propiedades de gráficos
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

        // Comandos
        public ICommand RefreshCommand { get; }
        public ICommand ActivateMotorCommand { get; }

        // Lista de horarios recibida vía MQTT
        private List<Horario> _horarios = new();
        public List<Horario> Horarios
        {
            get => _horarios;
            set { _horarios = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IMqttService mqttService, IConnectivityService connectivityService)
        {
            _mqttService = mqttService;
            _connectivityService = connectivityService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());

            // Suscribirse a los datos recibidos vía MQTT para actualizar las propiedades.
            _mqttService.OnSensorDataReceived += (s, data) =>
            {
                Temperature = data.Temperature;
                Humidity = data.Humidity;
                FoodLevel = data.Ultrasonido;
                Horarios = data.Horarios;
                UpdateHistory(data);
            };

            // Inicializa la conexión vía MQTT.
            InitializeRefresh();

            // Configurar un timer para reintentar la conexión cada 3 minutos.
            _refreshTimer = new System.Timers.Timer(180000);
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        // Método para iniciar la conexión con un retraso inicial.
        private async void InitializeRefresh()
        {
            await Task.Delay(3000);
            await RefreshDataAsync();
        }

        // Método para refrescar la conexión MQTT.
        public async Task RefreshDataAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                ConnectionStatus = "Sin conexión a Internet";
                return;
            }

            try
            {
                await _mqttService.ConnectAsync();
                ConnectionStatus = "Conexión remota (MQTT) - " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Error en la conexión MQTT: " + ex.Message;
            }
        }

        // Método para activar el motor usando MQTT.
        public async Task ActivateMotorAsync()
        {
            try
            {
                await _mqttService.PublishActivateMotorAsync();
            }
            catch (Exception ex)
            {
                // Aquí podrías agregar lógica de manejo de error o notificar al usuario.
            }
        }

        // Actualiza el historial de datos y reconstruye los gráficos.
        private void UpdateHistory(SensorData data)
        {
            // Si las colecciones tienen solo el valor predeterminado, se limpian.
            if (TemperatureHistory.Count == 1 && TemperatureHistory[0].ValueLabel == "0")
                TemperatureHistory.Clear();
            if (HumidityHistory.Count == 1 && HumidityHistory[0].ValueLabel == "0")
                HumidityHistory.Clear();
            if (FoodLevelHistory.Count == 1 && FoodLevelHistory[0].ValueLabel == "0")
                FoodLevelHistory.Clear();

            // Agregar nuevos datos con la hora actual como etiqueta.
            TemperatureHistory.Add(new ChartEntry((float)data.Temperature)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Temperature.ToString(),
                Color = SKColor.Parse("#FF0000")
            });
            HumidityHistory.Add(new ChartEntry((float)data.Humidity)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Humidity.ToString(),
                Color = SKColor.Parse("#0000FF")
            });
            FoodLevelHistory.Add(new ChartEntry((float)data.Ultrasonido)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Ultrasonido.ToString(),
                Color = SKColor.Parse("#00FF00")
            });

            // Limitar a 10 entradas eliminando las más antiguas.
            while (TemperatureHistory.Count > 10)
                TemperatureHistory.RemoveAt(0);
            while (HumidityHistory.Count > 10)
                HumidityHistory.RemoveAt(0);
            while (FoodLevelHistory.Count > 10)
                FoodLevelHistory.RemoveAt(0);

            // Reconstruir los gráficos.
            TemperatureChart = new LineChart { Entries = TemperatureHistory };
            HumidityChart = new LineChart { Entries = HumidityHistory };
            FoodLevelChart = new LineChart { Entries = FoodLevelHistory };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
