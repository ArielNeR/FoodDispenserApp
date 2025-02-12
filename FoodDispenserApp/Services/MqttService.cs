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

        public MqttService()
        {
            // Se utiliza MqttFactory (la forma actual recomendada) para crear el cliente.
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
                catch
                {
                    Console.WriteLine("Error al reconectar al broker MQTT. mqtt serviice");
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                try
                {
                    if (topic == "piscicultura/sensores")
                    {
                        // Deserializar datos de sensores
                        var sensorData = JsonSerializer.Deserialize<SensorData>(payload);
                        if (sensorData != null)
                        {
                            OnSensorDataReceived?.Invoke(this, sensorData);
                        }
                    }
                    else if (topic == "horarios/update")
                    {
                        // Deserializar la actualización de horarios
                        var horariosResponse = JsonSerializer.Deserialize<HorariosResponse>(payload);
                        if (horariosResponse != null)
                        {
                            var data = new SensorData { Horarios = horariosResponse.Horarios };
                            OnSensorDataReceived?.Invoke(this, data);
                        }
                    }
                }
                catch (Exception)
                {
                    // Manejar error de deserialización si es necesario
                }
            };

            // Configuración TLS: crear parámetros para habilitar TLS con SslProtocols.Tls12.
            var tlsParameters = new MqttClientTlsOptions
            {
                UseTls = true,
                SslProtocol = SslProtocols.Tls12,
                // Opcional: para propósitos de prueba, puedes aceptar todos los certificados.
                CertificateValidationHandler = context => true
            };

            // Configuración para conectarse al broker HiveMQ vía TLS.
            // Si se requieren credenciales, agregar el método .WithCredentials("usuario", "contraseña")
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("FoodDispenserAppClient")
                .WithTcpServer("04d1d89fd686436aba9da7fe351608aa.s1.eu.hivemq.cloud", 8883)
                .WithCredentials("dispensador", "zX7@pL9#fY2!mQv$T3^dR8&kW6*BsC1")
                .WithTlsOptions(tlsParameters) // Habilita TLS
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
                // Construir las opciones de suscripción usando el builder adecuado.
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("piscicultura/sensores")
                    .WithTopicFilter("horarios/update")
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
                    .WithPayload("") // Se puede enviar un payload específico si es necesario
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
        }
    }
}
