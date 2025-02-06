using FoodDispenserApp.Models;
using System.Net.Http.Json;

namespace FoodDispenserApp.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<double> GetTemperatureAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<TemperatureResponse>("temperature");
        if (response == null)
            throw new Exception("No se pudo deserializar la respuesta de temperatura.");
        return response.Temperature;
    }

    public async Task<double> GetHumidityAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<HumidityResponse>("humidity");
        if (response == null)
            throw new Exception("No se pudo deserializar la respuesta de humedad.");
        return response.Humidity;
    }

    public async Task<double> GetFoodLevelAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<FoodLevelResponse>("food_level");
        if (response == null)
            throw new Exception("No se pudo deserializar la respuesta del nivel de comida.");
        return response.Food_Level;
    }

    public async Task ActivateMotorAsync()
    {
        var response = await _httpClient.PostAsync("activate_motor", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Horario>> GetHorariosAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<HorariosResponse>("horarios");
        if (response == null)
            throw new Exception("No se pudo deserializar la respuesta de horarios.");
        return response.Horarios;
    }

    public async Task UpdateHorariosAsync(List<string> horarios)
    {
        var response = await _httpClient.PostAsJsonAsync("horarios", horarios);
        response.EnsureSuccessStatusCode();
    }
}
