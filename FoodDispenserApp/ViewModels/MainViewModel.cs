using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using Microcharts;
using SkiaSharp;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;

namespace FoodDispenserApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMqttService _mqttService;
        private readonly IConnectivityService _connectivityService;
        private bool _isRefreshing = false; // Para evitar refrescos concurrentes

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

        public ObservableCollection<ChartEntry> TemperatureHistory { get; set; } = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#FF0000")
            }
        };

        public ObservableCollection<ChartEntry> HumidityHistory { get; set; } = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#0000FF")
            }
        };

        public ObservableCollection<ChartEntry> FoodLevelHistory { get; set; } = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#00FF00")
            }
        };

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

        public ICommand RefreshCommand { get; }
        public ICommand ActivateMotorCommand { get; }

        private List<Horario> _horarios = new();
        public List<Horario> Horarios
        {
            get => _horarios;
            set { _horarios = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Para controlar la actualización (basada en timestamp)
        private DateTime _lastTimestamp = DateTime.MinValue;
        private bool _receivedValidData = false;


        public MainViewModel(IMqttService mqttService, IConnectivityService connectivityService)
        {
            _mqttService = mqttService;
            _connectivityService = connectivityService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());

            // Suscribirse a los datos vía MQTT
            _mqttService.OnSensorDataReceived += (s, data) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Actualizar horarios si el mensaje los contiene (sin importar los otros valores)
                    if (data.Horarios != null && data.Horarios.Count > 0)
                    {
                        Horarios = data.Horarios;
                    }

                    // Actualizar datos de sensores solo si son válidos (no todos 0)
                    bool isSensorDataValid = data.Temperature != 0 || data.Humidity != 0 || data.Ultrasonido != 0;
                    if (isSensorDataValid)
                    {
                        Temperature = data.Temperature;
                        Humidity = data.Humidity;
                        FoodLevel = data.Ultrasonido;
                        UpdateHistory(data);
                        _receivedValidData = true;
                    }
                });
            };


            // Conectar al iniciar
            InitializeRefresh();

            // Programar un refresco cada minuto usando Device.StartTimer en el hilo UI.
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                _ = RefreshDataAsync();
                return true; // Para continuar con el timer
            });
        }

        private async void InitializeRefresh()
        {
            await Task.Delay(1000);
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            if (_isRefreshing)
                return;

            _isRefreshing = true;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = "Sin conexión a Internet";
                });
                _isRefreshing = false;
                return;
            }

            try
            {
                await _mqttService.ConnectAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = "Conexión remota (MQTT) - " + DateTime.Now.ToString("HH:mm:ss");
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = "Error en la conexión MQTT: " + ex.Message;
                });
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        public async Task ActivateMotorAsync()
        {
            try
            {
                await _mqttService.PublishActivateMotorAsync();
            }
            catch (Exception ex)
            {
                // Opcional: Notificar error al usuario.
            }
        }

        private void UpdateHistory(SensorData data)
        {
            // Limpiar el historial si solo contiene el valor inicial.
            if (TemperatureHistory.Count == 1 && TemperatureHistory[0].ValueLabel == "0")
                TemperatureHistory.Clear();
            if (HumidityHistory.Count == 1 && HumidityHistory[0].ValueLabel == "0")
                HumidityHistory.Clear();
            if (FoodLevelHistory.Count == 1 && FoodLevelHistory[0].ValueLabel == "0")
                FoodLevelHistory.Clear();

            // Agregar la nueva entrada con la hora actual como etiqueta.
            TemperatureHistory.Add(new ChartEntry((float)data.Temperature)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = data.Temperature.ToString(),
                Color = SKColor.Parse("#FF0000")
            });
            HumidityHistory.Add(new ChartEntry((float)data.Humidity)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = data.Humidity.ToString(),
                Color = SKColor.Parse("#0000FF")
            });
            FoodLevelHistory.Add(new ChartEntry((float)data.Ultrasonido)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = data.Ultrasonido.ToString(),
                Color = SKColor.Parse("#00FF00")
            });

            // Mantener solo las 10 entradas más recientes.
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
