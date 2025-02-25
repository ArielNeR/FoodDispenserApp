using FoodDispenserApp.Models;
using MQTTnet;
using MQTTnet.Protocol;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;

namespace FoodDispenserApp.Services
{
    public class MqttService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        public bool IsConnected => _mqttClient.IsConnected;
        public event EventHandler<SensorData>? OnSensorDataReceived;
        public event EventHandler<HorariosResponse>? OnHorariosReceived;

        public MqttService()
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Cliente MQTT conectado. Suscribiendo a tópicos...");
                await SubscribeToTopics();
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("Cliente MQTT desconectado. Intentando reconectar en 5 segundos...");
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
                Console.WriteLine($"Mensaje recibido en tópico: {topic}, Payload: {payload}");

                try
                {
                    if (topic == "dispensador/sensores")
                    {
                        var sensorData = JsonSerializer.Deserialize<SensorData>(payload);
                        if (sensorData != null)
                        {
                            OnSensorDataReceived?.Invoke(this, sensorData);
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Datos de sensores recibidos pero vacíos.");
                        }
                    }
                    else if (topic == "dispensador/horarios")
                    {
                        var horariosResponse = JsonSerializer.Deserialize<HorariosResponse>(payload);
                        if (horariosResponse?.Horarios != null && horariosResponse.Horarios.Any())
                        {
                            OnHorariosReceived?.Invoke(this, horariosResponse);
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Datos de horarios recibidos pero vacíos.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al procesar mensaje MQTT: {ex.Message}");
                }
            };

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("FoodDispenserAppClient")
                .WithTcpServer("04d1d89fd686436aba9da7fe351608aa.s1.eu.hivemq.cloud", 8883)
                .WithCredentials("dispensador", "zX7@pL9#fY2!mQv$T3^dR8&kW6*BsC1")
                .WithTlsOptions(new MqttClientTlsOptions
                {
                    UseTls = true,
                    SslProtocol = SslProtocols.Tls12,
                    CertificateValidationHandler = context => true,
                })
                .WithCleanSession(false) // Cambiar a false para mantener el estado de la suscripción
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                Console.WriteLine("Conectando al broker MQTT...");
                await _mqttClient.ConnectAsync(_mqttOptions);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_mqttClient.IsConnected)
            {
                Console.WriteLine("Desconectando del broker MQTT...");
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
                Console.WriteLine("Suscripción a tópicos realizada: dispensador/sensores, dispensador/horarios");
            }
            else
            {
                Console.WriteLine("No se pudo suscribir: el cliente MQTT no está conectado.");
            }
        }

        public async Task PublishActivateMotorAsync()
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("commands/activate_motor")
                    .WithPayload("")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(false) // No retenido, ya que es un comando
                    .Build();

                await _mqttClient.PublishAsync(message);
                Console.WriteLine("Mensaje de activación del motor publicado.");
            }
        }

        public async Task PublishHorariosAsync(List<Horario> horarios)
        {
            if (_mqttClient.IsConnected)
            {
                var payload = JsonSerializer.Serialize(new { horarios });
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("dispensador/horarios")
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(true) // Asegurar que el mensaje sea retenido
                    .Build();

                await _mqttClient.PublishAsync(message);
                Console.WriteLine($"Horarios publicados en dispensador/horarios: {payload}");
            }
            else
            {
                Console.WriteLine("No se pudo publicar horarios: el cliente MQTT no está conectado.");
            }
        }
    }
}