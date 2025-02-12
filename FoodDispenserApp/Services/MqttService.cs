using FoodDispenserApp.Models;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDispenserApp.Services
{
    public class MqttService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        public bool IsConnected => _mqttClient.IsConnected;
        public event EventHandler<SensorData>? OnSensorDataReceived;

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
                    await ConnectAsync();
                }
                catch
                {
                    // Registrar error si es necesario.
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if (topic == "piscicultura/sensores")
                {
                    try
                    {
                        var sensorData = JsonSerializer.Deserialize<SensorData>(payload);
                        if (sensorData != null)
                        {
                            OnSensorDataReceived?.Invoke(this, sensorData);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error al procesar el mensaje MQTT");
                    }
                }
                await Task.CompletedTask;
            };

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("FoodDispenserAppClient")
                .WithTcpServer("04d1d89fd686436aba9da7fe351608aa.s1.eu.hivemq.cloud", 8883)
                .WithCredentials("dispensador", "zX7@pL9#fY2!mQv$T3^dR8&kW6*BsC1")
                .WithTlsOptions(new MqttClientTlsOptions
                {
                    UseTls = true,
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                    // Para pruebas: acepta cualquier certificado (no recomendado en producción)
                    CertificateValidationHandler = context => true,
                })
                .WithCleanSession()
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
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic("piscicultura/sensores")
                    .Build();

                await _mqttClient.SubscribeAsync(topicFilter);
            }
        }

        public async Task PublishHorariosUpdateAsync(List<Horario> horarios)
        {
            // Envolver el listado en un objeto para que el JSON tenga la propiedad "horarios"
            var payload = JsonSerializer.Serialize(new { horarios = horarios });
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("piscicultura/horarios/update")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient.PublishAsync(message);
        }
    }
}
