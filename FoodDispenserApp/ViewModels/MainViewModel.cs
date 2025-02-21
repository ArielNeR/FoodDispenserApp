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
        private bool _isRefreshing = false; // Evita refrescos concurrentes

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

        public ObservableCollection<ChartEntry> TemperatureHistory { get; set; } = new ObservableCollection<ChartEntry>();
        public ObservableCollection<ChartEntry> HumidityHistory { get; set; } = new ObservableCollection<ChartEntry>();
        public ObservableCollection<ChartEntry> FoodLevelHistory { get; set; } = new ObservableCollection<ChartEntry>();

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

        public MainViewModel(IMqttService mqttService, IConnectivityService connectivityService)
        {
            _mqttService = mqttService;
            _connectivityService = connectivityService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());

            // Suscribirse a los eventos MQTT
            _mqttService.OnSensorDataReceived += (sender, sensorData) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Temperature = sensorData.Temperature;
                    Humidity = sensorData.Humidity;
                    FoodLevel = sensorData.Ultrasonido;

                    UpdateHistory(sensorData);
                });
            };

            _mqttService.OnHorariosReceived += (sender, horariosResponse) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (horariosResponse?.Horarios != null && horariosResponse.Horarios.Any())
                    {
                        Horarios.Clear();
                        Horarios.AddRange(horariosResponse.Horarios);
                    }
                });
            };


            // Conectar y mantener la conexión activa
            InitializeRefresh();
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                _ = RefreshDataAsync();
                return true; // Continuar con el timer
            });
        }

        private async void InitializeRefresh()
        {
            await Task.Delay(1000);
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            if (_isRefreshing) return;

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
                if (!_mqttService.IsConnected)
                {
                    await _mqttService.ConnectAsync();

                    if (_mqttService.IsConnected)
                    {
                        await _mqttService.SubscribeToTopics();
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = $"Conectado al broker MQTT - {DateTime.Now:HH:mm:ss}";
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = $"Error en la conexión MQTT: {ex.Message}";
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
                if (_mqttService.IsConnected)
                {
                    await _mqttService.PublishActivateMotorAsync();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ConnectionStatus = "Motor activado";
                    });
                }
                else
                {
                    ConnectionStatus = "No hay conexión con el broker MQTT";
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = $"Error al activar el motor: {ex.Message}";
                });
            }
        }

        private void UpdateHistory(SensorData data)
        {
            if (data == null)
                return;

            // Asegurar que las listas no estén vacías antes de operar
            if (!TemperatureHistory.Any()) TemperatureHistory.Clear();
            if (!HumidityHistory.Any()) HumidityHistory.Clear();
            if (!FoodLevelHistory.Any()) FoodLevelHistory.Clear();

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

            // **Verificación adicional** antes de intentar eliminar elementos
            if (TemperatureHistory.Count > 10) TemperatureHistory.RemoveAt(0);
            if (HumidityHistory.Count > 10) HumidityHistory.RemoveAt(0);
            if (FoodLevelHistory.Count > 10) FoodLevelHistory.RemoveAt(0);

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
