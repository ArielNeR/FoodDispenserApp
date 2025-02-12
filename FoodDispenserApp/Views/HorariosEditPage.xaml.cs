using FoodDispenserApp.Models;
using FoodDispenserApp.ViewModels;

namespace FoodDispenserApp.Views;

public partial class HorariosEditPage : ContentPage
{
    public HorariosEditPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnAddHorarioClicked(object sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            // Agregar un nuevo horario con valores predeterminados
            vm.Horarios.Add(new Horario { Hora = 8, Minuto = 0, Duracion = 3 });
            // Reasigna la lista para forzar la actualización de la UI
            vm.Horarios = new List<Horario>(vm.Horarios);
        }
    }

    private void OnDeleteHorarioClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Horario horario)
        {
            if (BindingContext is MainViewModel vm)
            {
                vm.Horarios.Remove(horario);
                vm.Horarios = new List<Horario>(vm.Horarios);
            }
        }
    }
}
