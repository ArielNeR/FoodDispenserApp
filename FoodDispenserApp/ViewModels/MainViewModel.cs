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

        private string _connectionStatus = "Desconocido";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        // Historial de datos para gráficos, inicializados con un valor predeterminado
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

        public ICommand RefreshCommand { get; }
        public ICommand ActivateMotorCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private List<Horario> _horarios = new();
        public List<Horario> Horarios
        {
            get => _horarios;
            set { _horarios = value; OnPropertyChanged(); }
        }

        // Comando para guardar los horarios
        public ICommand SaveHorariosCommand { get; }

        public MainViewModel(IApiService apiService,
                             IMqttService mqttService,
                             IConnectivityService connectivityService)
        {
            _apiService = apiService;
            _mqttService = mqttService;
            _connectivityService = connectivityService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            ActivateMotorCommand = new Command(async () => await ActivateMotorAsync());
            SaveHorariosCommand = new Command(async () => await SaveHorariosAsync());

            // Suscribirse a datos vía MQTT (actualiza los datos y el historial)
            _mqttService.OnSensorDataReceived += (s, data) =>
            {
                Temperature = data.Temperature;
                Humidity = data.Humidity;
                FoodLevel = data.FoodLevel;
                Horarios = data.Horarios;
                UpdateHistory(data);
            };

            ConnectionStatus = "Esperando actualización...";

            InitializeRefresh();

            _refreshTimer = new System.Timers.Timer(180000); // 3 minutos
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        private async Task SaveHorariosAsync()
        {
            try
            {
                await _apiService.UpdateHorariosAsync(Horarios);
                await Application.Current.MainPage.DisplayAlert("Éxito", "Horarios actualizados correctamente", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron actualizar los horarios: " + ex.Message, "OK");
            }
        }

        // Método para iniciar el primer refresco con un retraso de 3 segundos
        private async void InitializeRefresh()
        {
            await Task.Delay(3000);
            await RefreshDataAsync();
        }

        // Método para refrescar datos y actualizar el estado de conexión
        public async Task RefreshDataAsync()
        {
            // Primero, verificar si hay conexión a Internet
            var netAccess = Connectivity.Current.NetworkAccess;
            if (netAccess != NetworkAccess.Internet)
            {
                ConnectionStatus = "Sin conexión a Internet";
                return;
            }

            // Si hay internet, determinamos si es conexión local o no
            if (_connectivityService.IsLocal)
            {
                try
                {
                    // Actualizar datos vía HTTP
                    Temperature = await _apiService.GetTemperatureAsync();
                    Humidity = await _apiService.GetHumidityAsync();
                    FoodLevel = await _apiService.GetFoodLevelAsync();
                    Horarios = await _apiService.GetHorariosAsync();

                    // Construir un objeto SensorData para actualizar el historial
                    SensorData data = new SensorData
                    {
                        Temperature = Temperature,
                        Humidity = Humidity,
                        FoodLevel = FoodLevel,
                        Horarios = Horarios
                    };
                    UpdateHistory(data);

                    // Actualizar el estado de conexión con éxito
                    ConnectionStatus = "Conexión exitosa (HTTP Local) - " + DateTime.Now.ToString("HH:mm:ss");
                }
                catch (Exception ex)
                {
                    ConnectionStatus = "Error en la conexión HTTP: " + ex.Message;
                }
            }
            else
            {
                // En este caso, hay conexión a Internet pero no estamos en la red local.
                // Se intenta establecer la conexión vía MQTT.
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
        }


        // Método para actualizar el historial y los gráficos, limitando a 10 datos
        private void UpdateHistory(SensorData data)
        {
            // Si las colecciones solo tienen el valor predeterminado, se limpian para reemplazarlo
            if (TemperatureHistory.Count == 1 && TemperatureHistory[0].ValueLabel == "0")
                TemperatureHistory.Clear();
            if (HumidityHistory.Count == 1 && HumidityHistory[0].ValueLabel == "0")
                HumidityHistory.Clear();
            if (FoodLevelHistory.Count == 1 && FoodLevelHistory[0].ValueLabel == "0")
                FoodLevelHistory.Clear();

            // Agregar los nuevos datos con la hora actual como etiqueta
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
            FoodLevelHistory.Add(new ChartEntry((float)data.FoodLevel)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.FoodLevel.ToString(),
                Color = SKColor.Parse("#00FF00")
            });

            // Limitar la cantidad de entradas a 10 (eliminando las más antiguas)
            while (TemperatureHistory.Count > 10)
                TemperatureHistory.RemoveAt(0);
            while (HumidityHistory.Count > 10)
                HumidityHistory.RemoveAt(0);
            while (FoodLevelHistory.Count > 10)
                FoodLevelHistory.RemoveAt(0);

            // Actualizar (reconstruir) los gráficos para que reflejen los cambios
            TemperatureChart = new LineChart { Entries = TemperatureHistory };
            HumidityChart = new LineChart { Entries = HumidityHistory };
            FoodLevelChart = new LineChart { Entries = FoodLevelHistory };
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
