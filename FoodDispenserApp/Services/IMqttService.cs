using FoodDispenserApp.Models;

namespace FoodDispenserApp.Services;

public interface IMqttService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected { get; }
    Task SubscribeToTopics();
    event EventHandler<SensorData>? OnSensorDataReceived;
    Task PublishActivateMotorAsync();
}
