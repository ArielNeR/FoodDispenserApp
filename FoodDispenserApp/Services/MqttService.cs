using FoodDispenserApp.Models;
using MQTTnet;
using MQTTnet.Protocol;
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
            // Asegúrate de tener instalado el paquete MQTTnet y de incluir "using MQTTnet;"
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configurar los manejadores de eventos usando lambdas asíncronas sin retornar Task.CompletedTask manualmente
            _mqttClient.ConnectedAsync += async e =>
            {
                // Al conectar, suscribirse a los tópicos necesarios
                await SubscribeToTopics();
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                // Intento de reconexión automático después de 5 segundos
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await _mqttClient.ConnectAsync(_mqttOptions);
                }
                catch
                {
                    // Manejo de error de reconexión (puedes registrar el error aquí)
                }
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                // Se asume que la API publica un JSON completo en el tópico "sensor/updates"
                if (topic == "sensor/updates")
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
                        // Manejo de error al parsear el JSON
                    }
                }
            };

            // Configuración del cliente MQTT (usa un broker público, por ejemplo, broker.hivemq.com)
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("FoodDispenserAppClient")
                .WithTcpServer("broker.hivemq.com", 1883)
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
                // Suscribirse al tópico donde se envían las actualizaciones de sensores
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic("sensor/updates")
                    .Build();

                await _mqttClient.SubscribeAsync(topicFilter);
            }
        }

        public async Task PublishActivateMotorAsync()
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("commands/activate_motor")
                    .WithPayload("") // Se puede enviar un payload vacío o un comando específico
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag(false)
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
        }
    }
}
