using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FoodDispenserApp.Models
{
    public class SensorData
    {
        // Campos del mensaje de sensores (tópico: piscicultura/sensores)
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("temperatura")]
        public double Temperature { get; set; }

        [JsonPropertyName("humedad")]
        public double Humidity { get; set; }

        [JsonPropertyName("ultrasonido")]
        public double Ultrasonido { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        // Propiedad para la actualización de horarios (tópico: horarios/update)
        public List<Horario> Horarios { get; set; } = new();
    }
}
