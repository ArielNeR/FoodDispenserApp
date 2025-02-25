using FoodDispenserApp.Models;
using MQTTnet;
using MQTTnet.Protocol;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodDispenserApp.Services
{
    public class MqttService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        public bool IsConnected => _mqttClient.IsConnected;
        public event EventHandler<SensorData>? OnSensorDataReceived;
        public event EventHandler<HorariosResponse>? OnHorariosReceived;

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public MqttService()
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ConnectedAsync += async e =>
            {
                await SubscribeToTopics();
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await _mqttClient.ConnectAsync(_mqttOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al reconectar al broker MQTT: {ex.Message}");
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                try
                {
                    if (topic == "dispensador/sensores")
                    {
                        var sensorData = JsonSerializer.Deserialize<SensorData>(payload);
                        if (sensorData != null)
                        {
                            OnSensorDataReceived?.Invoke(this, sensorData);
                        }
                    }
                    else if (topic == "dispensador/horarios")
                    {
                        var horariosList = JsonSerializer.Deserialize<List<Horario>>(payload, SerializerOptions);
                        var horariosResponse = new HorariosResponse { Horarios = horariosList ?? new List<Horario>() };
                        if (horariosResponse.Horarios.Any())
                        {
                            OnHorariosReceived?.Invoke(this, horariosResponse);
                        }
                    }
                }
                catch { /* Ignorar errores de deserialización para evitar bloqueos */ }
            };

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("FoodDispenserAppClient")
                .WithTcpServer("04d1d89fd686436aba9da7fe351608aa.s1.eu.hivemq.cloud", 8883)
                .WithCredentials("dispensador", "zX7@pL9#fY2!mQv$T3^dR8&kW6*BsC1")
                .WithTlsOptions(new MqttClientTlsOptions
                {
                    UseTls = true,
                    SslProtocol = SslProtocols.Tls12,
                    CertificateValidationHandler = _ => true
                })
                .WithCleanSession(false)
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        public async Task SubscribeToTopics()
        {
            if (_mqttClient.IsConnected)
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("dispensador/sensores")
                    .WithTopicFilter("dispensador/horarios")
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions);
            }
        }

        public async Task PublishActivateMotorAsync()
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("commands/activate_motor")
                    .WithPayload(Array.Empty<byte>())
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
        }

        public async Task PublishHorariosAsync(List<Horario> horarios)
        {
            if (_mqttClient.IsConnected)
            {
                var payload = JsonSerializer.Serialize(horarios, SerializerOptions);
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("dispensador/horarios")
                    .WithPayload(Encoding.UTF8.GetBytes(payload))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(true)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
        }
    }
}