using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;

namespace FoodDispenserApp.ViewModels
{
    public class HorariosViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IMqttService _mqttService;

        public ObservableCollection<Horario> Horarios { get; } // Colección compartida

        public ICommand LoadHorariosCommand { get; }
        public ICommand UpdateHorariosCommand { get; }
        public ICommand EditHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public HorariosViewModel(IApiService apiService, IMqttService mqttService, ObservableCollection<Horario> horarios)
        {
            _apiService = apiService;
            _mqttService = mqttService;
            Horarios = horarios;

            LoadHorariosCommand = new Command(async () => await LoadHorariosAsync());
            UpdateHorariosCommand = new Command(async () => await UpdateHorariosAsync());
            EditHorariosCommand = new Command(OnEditHorarios);
        }

        private async Task LoadHorariosAsync()
        {
            // No necesitamos reconectar aquí; MainViewModel ya maneja la conexión MQTT
            Console.WriteLine("Horarios ya están siendo gestionados por MainViewModel.");
        }

        private async Task UpdateHorariosAsync()
        {
            try
            {
                Console.WriteLine($"Publicando horarios: {JsonSerializer.Serialize(Horarios.ToList())}");
                await _mqttService.PublishHorariosAsync(Horarios.ToList());
                await Application.Current.MainPage.DisplayAlert("Éxito", "Horarios guardados correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar horarios: {ex.Message}", "OK");
            }
        }

        private void OnEditHorarios()
        {
            // Lógica para edición si es necesario
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}