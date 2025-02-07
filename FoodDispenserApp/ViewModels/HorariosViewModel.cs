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
        private readonly IApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Colección observable para mostrar la lista de horarios
        public ObservableCollection<Horario> Horarios { get; set; } = new ObservableCollection<Horario>();

        // Comandos para cargar y actualizar horarios
        public ICommand LoadHorariosCommand { get; }
        public ICommand UpdateHorariosCommand { get; }
        public ICommand EditHorariosCommand { get; } // Aquí puedes implementar la lógica de edición

        public HorariosViewModel(IApiService apiService)
        {
            _apiService = apiService;
            LoadHorariosCommand = new Command(async () => await LoadHorariosAsync());
            UpdateHorariosCommand = new Command(async () => await UpdateHorariosAsync());
            EditHorariosCommand = new Command(OnEditHorarios);
            // Cargar horarios al iniciar
            Task.Run(async () => await LoadHorariosAsync());
        }

        private async Task LoadHorariosAsync()
        {
            try
            {
                var horariosList = await _apiService.GetHorariosAsync();
                Horarios.Clear();
                foreach (var h in horariosList)
                {
                    Horarios.Add(h);
                }
            }
            catch (Exception ex)
            {
                // Aquí podrías mostrar un mensaje o registrar el error
            }
        }

        private async Task UpdateHorariosAsync()
        {
            try
            {
                // Enviar la lista actual de horarios a la API para actualizar
                //await _apiService.UpdateHorariosAsync(Horarios.ToList());
                // Vuelve a cargar para confirmar la actualización
                await LoadHorariosAsync();
            }
            catch (Exception ex)
            {
                // Manejo de error (mostrar mensaje, etc.)
            }
        }

        private void OnEditHorarios()
        {
            // Aquí puedes implementar la lógica para editar un horario:
            // Por ejemplo, navegar a una página de edición o mostrar un diálogo modal.
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
