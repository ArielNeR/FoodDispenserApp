using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;

namespace FoodDispenserApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMqttService _mqttService;
        private readonly IConnectivityService _connectivityService;
        private readonly BackgroundDataService _backgroundService;
        private bool _isRefreshing = false;

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

        public ObservableCollection<ChartEntry> TemperatureHistory { get; set; } = new();
        public ObservableCollection<ChartEntry> HumidityHistory { get; set; } = new();
        public ObservableCollection<ChartEntry> FoodLevelHistory { get; set; } = new();

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

        private ObservableCollection<Horario> _horarios = new();
        public ObservableCollection<Horario> Horarios
        {
            get => _horarios;
            set { _horarios = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ActivateMotorCommand { get; }
        public ICommand SaveHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IMqttService mqttService, IConnectivityService connectivityService, BackgroundDataService backgroundService)
        {
            _mqttService = mqttService;
            _connectivityService = connectivityService;
            _backgroundService = backgroundService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());
            SaveHorariosCommand = new Command(async () => await SaveHorariosAsync());

            TemperatureChart = new LineChart { Entries = TemperatureHistory };
            HumidityChart = new LineChart { Entries = HumidityHistory };
            FoodLevelChart = new LineChart { Entries = FoodLevelHistory };

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
                        foreach (var horario in horariosResponse.Horarios)
                        {
                            Horarios.Add(horario);
                        }
                    }
                });
            };

            InitializeRefresh();
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                _ = RefreshDataAsync();
                return true;
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

            try
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    ConnectionStatus = "Sin conexión a Internet";
                    await Application.Current.MainPage.DisplayAlert("Error", "Sin conexión a Internet", "OK");
                    return;
                }

                // Usamos el método asíncrono para verificar la conectividad local
                bool isLocal = await _connectivityService.CheckLocalConnectivityAsync();
                ConnectionStatus = isLocal ? "Conectado en modo local" : "Conectado en modo remoto";

                if (!_mqttService.IsConnected)
                {
                    await _mqttService.ConnectAsync();
                    if (_mqttService.IsConnected)
                    {
                        await _mqttService.SubscribeToTopics();
                    }
                }
                ConnectionStatus = $"Conectado al broker MQTT - {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error en la conexión MQTT: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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
                    ConnectionStatus = "Motor activado";
                }
                else
                {
                    ConnectionStatus = "No hay conexión con el broker MQTT";
                    await Application.Current.MainPage.DisplayAlert("Error", "No hay conexión con el broker MQTT", "OK");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error al activar el motor: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task SaveHorariosAsync()
        {
            try
            {
                await _mqttService.PublishHorariosAsync(Horarios.ToList());
                ConnectionStatus = "Horarios guardados correctamente";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error al guardar horarios: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void UpdateHistory(SensorData data)
        {
            if (data == null) return;

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

            if (TemperatureHistory.Count > 10) TemperatureHistory.RemoveAt(0);
            if (HumidityHistory.Count > 10) HumidityHistory.RemoveAt(0);
            if (FoodLevelHistory.Count > 10) FoodLevelHistory.RemoveAt(0);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}