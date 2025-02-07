using FoodDispenserApp.Models;

namespace FoodDispenserApp.Services;

public interface IApiService
{
    Task<double> GetTemperatureAsync();
    Task<double> GetHumidityAsync();
    Task<double> GetFoodLevelAsync();
    Task ActivateMotorAsync();
    Task<List<Horario>> GetHorariosAsync();
    Task UpdateHorariosAsync(List<Horario> horarios);
}
