using FoodDispenserApp.Models;
using FoodDispenserApp.ViewModels;

namespace FoodDispenserApp.Views;

public partial class HorariosEditPage : ContentPage
{
    private readonly HorariosViewModel _viewModel;

    public HorariosEditPage(HorariosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnAddHorarioClicked(object sender, EventArgs e)
    {
        _viewModel.Horarios.Add(new Horario { Hora = 8, Minuto = 0, Duracion = 3 });
    }

    private void OnDeleteHorarioClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Horario horario)
        {
            _viewModel.Horarios.Remove(horario);
        }
    }
}