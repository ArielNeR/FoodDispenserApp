using System;
using System.Collections.Generic;
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
        private readonly IMqttService _mqttService;
        private readonly IConnectivityService _connectivityService;
        private System.Timers.Timer _refreshTimer;  // Opcional: para reintentos o actualización del estado

        // Datos de sensores
        private double _temperatura;
        public double Temperatura
        {
            get => _temperatura;
            set { _temperatura = value; OnPropertyChanged(); }
        }

        private double _humedad;
        public double Humedad
        {
            get => _humedad;
            set { _humedad = value; OnPropertyChanged(); }
        }

        private double _ultrasonido;
        public double Ultrasonido
        {
            get => _ultrasonido;
            set { _ultrasonido = value; OnPropertyChanged(); }
        }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); }
        }

        private string _connectionStatus = "Desconocido";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        // Historial para gráficos (limitado a 10 entradas)
        private ObservableCollection<ChartEntry> _temperaturaHistory = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#FF0000")
            }
        };
        public ObservableCollection<ChartEntry> TemperaturaHistory
        {
            get => _temperaturaHistory;
            set { _temperaturaHistory = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChartEntry> _humedadHistory = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#0000FF")
            }
        };
        public ObservableCollection<ChartEntry> HumedadHistory
        {
            get => _humedadHistory;
            set { _humedadHistory = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ChartEntry> _ultrasonidoHistory = new ObservableCollection<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Sin datos",
                ValueLabel = "0",
                Color = SKColor.Parse("#00FF00")
            }
        };
        public ObservableCollection<ChartEntry> UltrasonidoHistory
        {
            get => _ultrasonidoHistory;
            set { _ultrasonidoHistory = value; OnPropertyChanged(); }
        }

        // Gráficos
        private Chart _temperaturaChart;
        public Chart TemperaturaChart
        {
            get => _temperaturaChart;
            set { _temperaturaChart = value; OnPropertyChanged(); }
        }

        private Chart _humedadChart;
        public Chart HumedadChart
        {
            get => _humedadChart;
            set { _humedadChart = value; OnPropertyChanged(); }
        }

        private Chart _ultrasonidoChart;
        public Chart UltrasonidoChart
        {
            get => _ultrasonidoChart;
            set { _ultrasonidoChart = value; OnPropertyChanged(); }
        }

        // Horarios (editable localmente)
        private List<Horario> _horarios = new List<Horario>();
        public List<Horario> Horarios
        {
            get => _horarios;
            set { _horarios = value; OnPropertyChanged(); }
        }

        // Comando para refrescar la conexión (MQTT)
        public ICommand RefreshCommand { get; }
        // Comando para guardar (publicar) la actualización de horarios vía MQTT
        public ICommand SaveHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(IMqttService mqttService, IConnectivityService connectivityService)
        {
            _mqttService = mqttService;
            _connectivityService = connectivityService;

            RefreshCommand = new Command(async () => await RefreshDataAsync());
            SaveHorariosCommand = new Command(async () => await SaveHorariosAsync());

            // Suscribirse a los datos que llegan vía MQTT
            _mqttService.OnSensorDataReceived += (s, data) =>
            {
                Temperatura = data.Temperatura;
                Humedad = data.Humedad;
                Ultrasonido = data.Ultrasonido;
                Timestamp = data.Timestamp;
                // Los horarios se manejan de forma local; si el mensaje MQTT los incluyera, se actualizarían aquí.
                UpdateHistory(data);
            };

            ConnectionStatus = "Conectando vía MQTT...";

            InitializeRefresh();

            // Opcional: timer para reintentos o actualización del estado
            _refreshTimer = new System.Timers.Timer(180000); // 3 minutos
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        private async void InitializeRefresh()
        {
            await Task.Delay(3000);
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
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

        private void UpdateHistory(SensorData data)
        {
            if (TemperaturaHistory.Count == 1 && TemperaturaHistory[0].ValueLabel == "0")
                TemperaturaHistory.Clear();
            if (HumedadHistory.Count == 1 && HumedadHistory[0].ValueLabel == "0")
                HumedadHistory.Clear();
            if (UltrasonidoHistory.Count == 1 && UltrasonidoHistory[0].ValueLabel == "0")
                UltrasonidoHistory.Clear();

            TemperaturaHistory.Add(new ChartEntry((float)data.Temperatura)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Temperatura.ToString(),
                Color = SKColor.Parse("#FF0000")
            });
            HumedadHistory.Add(new ChartEntry((float)data.Humedad)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Humedad.ToString(),
                Color = SKColor.Parse("#0000FF")
            });
            UltrasonidoHistory.Add(new ChartEntry((float)data.Ultrasonido)
            {
                Label = DateTime.Now.ToString("HH:mm"),
                ValueLabel = data.Ultrasonido.ToString(),
                Color = SKColor.Parse("#00FF00")
            });

            while (TemperaturaHistory.Count > 10)
                TemperaturaHistory.RemoveAt(0);
            while (HumedadHistory.Count > 10)
                HumedadHistory.RemoveAt(0);
            while (UltrasonidoHistory.Count > 10)
                UltrasonidoHistory.RemoveAt(0);

            TemperaturaChart = new Microcharts.LineChart { Entries = TemperaturaHistory };
            HumedadChart = new Microcharts.LineChart { Entries = HumedadHistory };
            UltrasonidoChart = new Microcharts.LineChart { Entries = UltrasonidoHistory };
        }

        private async Task SaveHorariosAsync()
        {
            try
            {
                await _mqttService.PublishHorariosUpdateAsync(Horarios);
                await App.Current.MainPage.DisplayAlert("Éxito", "Horarios actualizados correctamente", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No se pudieron actualizar los horarios: " + ex.Message, "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
