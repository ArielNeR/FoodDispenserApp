using FoodDispenserApp.Models;

namespace FoodDispenserApp.Services
{
    public interface IMqttService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        bool IsConnected { get; }
        Task SubscribeToTopics();
        event EventHandler<SensorData>? OnSensorDataReceived;

        // Método para enviar actualizaciones de horarios vía MQTT.
        Task PublishHorariosUpdateAsync(List<Horario> horarios);
    }
}
