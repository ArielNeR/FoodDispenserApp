namespace FoodDispenserApp.Models;

public class SensorData
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double FoodLevel { get; set; }
    // Se asume que los horarios de dispensación se envían como lista de cadenas (por ejemplo, "08:00", "12:00", "18:00")
    public List<Horario> Horarios { get; set; } = new();
}
