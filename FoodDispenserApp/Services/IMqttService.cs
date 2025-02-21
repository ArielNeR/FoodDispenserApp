using FoodDispenserApp.Models;

public interface IMqttService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected { get; }
    Task SubscribeToTopics();
    event EventHandler<SensorData>? OnSensorDataReceived;
    event EventHandler<HorariosResponse>? OnHorariosReceived;

    Task PublishHorariosAsync(List<Horario> horarios);
    Task PublishActivateMotorAsync();
}
