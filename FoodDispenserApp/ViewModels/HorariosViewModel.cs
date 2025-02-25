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

        public ObservableCollection<Horario> Horarios { get; set; } = new();

        public ICommand LoadHorariosCommand { get; }
        public ICommand UpdateHorariosCommand { get; }
        public ICommand EditHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public HorariosViewModel(IApiService apiService, IMqttService mqttService)
        {
            _apiService = apiService;
            _mqttService = mqttService;

            LoadHorariosCommand = new Command(async () => await LoadHorariosAsync());
            UpdateHorariosCommand = new Command(async () => await UpdateHorariosAsync());
            EditHorariosCommand = new Command(OnEditHorarios);

            _mqttService.OnHorariosReceived += (sender, horariosResponse) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Horarios.Clear();
                    foreach (var h in horariosResponse.Horarios)
                    {
                        Horarios.Add(h);
                    }
                });
            };

            Task.Run(async () => await LoadHorariosAsync());
        }

        private async Task LoadHorariosAsync()
        {
            try
            {
                var horariosList = await _apiService.GetHorariosAsync();
                Horarios.Clear();
                if (horariosList != null && horariosList.Any())
                {
                    foreach (var h in horariosList)
                    {
                        Horarios.Add(h);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "No se encontraron horarios.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los horarios: {ex.Message}", "OK");
            }
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
            // Lógica para edición si es necesario (puede navegar a HorariosEditPage desde la UI)
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}