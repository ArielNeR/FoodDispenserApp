using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FoodDispenserApp.Models
{
    public class SensorData
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("temperatura")]
        public double Temperatura { get; set; }

        [JsonPropertyName("humedad")]
        public double Humedad { get; set; }

        [JsonPropertyName("ultrasonido")]
        public double Ultrasonido { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        // Aunque el mensaje MQTT no incluya horarios, esta propiedad permite
        // mantener la edición local de horarios.
        public List<Horario> Horarios { get; set; } = new();
    }
}
