using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls;

namespace FoodDispenserApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMqttService _mqttService;
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

        private string _connectionStatus = "Esperando conexión MQTT...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChartEntry> TemperatureHistory { get; } = new();
        public ObservableCollection<ChartEntry> HumidityHistory { get; } = new();
        public ObservableCollection<ChartEntry> FoodLevelHistory { get; } = new();

        private Chart _temperatureChart;
        public Chart TemperatureChart
        {
            get => _temperatureChart;
            private set { _temperatureChart = value; OnPropertyChanged(); }
        }

        private Chart _humidityChart;
        public Chart HumidityChart
        {
            get => _humidityChart;
            private set { _humidityChart = value; OnPropertyChanged(); }
        }

        private Chart _foodLevelChart;
        public Chart FoodLevelChart
        {
            get => _foodLevelChart;
            private set { _foodLevelChart = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Horario> Horarios { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ActivateMotorCommand { get; }
        public ICommand SaveHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IMqttService mqttService, ObservableCollection<Horario> horarios)
        {
            _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
            Horarios = horarios ?? throw new ArgumentNullException(nameof(horarios));

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());
            SaveHorariosCommand = new Command(async () => await SaveHorariosAsync());

            // Inicializar gráficos vacíos
            TemperatureChart = new LineChart { Entries = TemperatureHistory };
            HumidityChart = new LineChart { Entries = HumidityHistory };
            FoodLevelChart = new LineChart { Entries = FoodLevelHistory };

            // Configurar eventos MQTT
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
                    Horarios.Clear();
                    if (horariosResponse?.Horarios != null && horariosResponse.Horarios.Any())
                    {
                        foreach (var horario in horariosResponse.Horarios)
                        {
                            Horarios.Add(horario);
                        }
                    }
                });
            };

            // Iniciar conexión MQTT de forma asíncrona
            Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

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
                ConnectionStatus = $"Conectado al broker MQTT - {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error en la conexión MQTT: {ex.Message}";
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
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error al activar el motor: {ex.Message}";
            }
        }

        private async Task SaveHorariosAsync()
        {
            try
            {
                if (_mqttService.IsConnected)
                {
                    await _mqttService.PublishHorariosAsync(Horarios.ToList());
                    ConnectionStatus = "Horarios guardados correctamente";
                }
                else
                {
                    ConnectionStatus = "No hay conexión con el broker MQTT";
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error al guardar horarios: {ex.Message}";
            }
        }

        private void UpdateHistory(SensorData data)
        {
            if (data == null) return;

            TemperatureHistory.Add(new ChartEntry((float)data.Temperature)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = $"{data.Temperature:F1}",
                Color = SKColor.Parse("#FF0000")
            });

            HumidityHistory.Add(new ChartEntry((float)data.Humidity)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = $"{data.Humidity:F1}",
                Color = SKColor.Parse("#0000FF")
            });

            FoodLevelHistory.Add(new ChartEntry((float)data.Ultrasonido)
            {
                Label = data.Timestamp.ToString("HH:mm"),
                ValueLabel = $"{data.Ultrasonido:F1}",
                Color = SKColor.Parse("#00FF00")
            });

            const int maxEntries = 10;
            while (TemperatureHistory.Count > maxEntries) TemperatureHistory.RemoveAt(0);
            while (HumidityHistory.Count > maxEntries) HumidityHistory.RemoveAt(0);
            while (FoodLevelHistory.Count > maxEntries) FoodLevelHistory.RemoveAt(0);

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