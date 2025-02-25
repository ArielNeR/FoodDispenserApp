using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FoodDispenserApp.Models;
using FoodDispenserApp.Services;

namespace FoodDispenserApp.ViewModels
{
    public class HorariosViewModel : INotifyPropertyChanged
    {
        private readonly IMqttService _mqttService;

        public ObservableCollection<Horario> Horarios { get; }

        public ICommand UpdateHorariosCommand { get; }
        public ICommand EditHorariosCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public HorariosViewModel(IMqttService mqttService, ObservableCollection<Horario> horarios)
        {
            _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
            Horarios = horarios ?? throw new ArgumentNullException(nameof(horarios));

            UpdateHorariosCommand = new Command(async () => await UpdateHorariosAsync());
            EditHorariosCommand = new Command(OnEditHorarios);
        }

        private async Task UpdateHorariosAsync()
        {
            try
            {
                if (_mqttService.IsConnected)
                {
                    await _mqttService.PublishHorariosAsync(Horarios.ToList());
                }
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