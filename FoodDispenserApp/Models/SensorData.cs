using System;
using System.Text.Json.Serialization;

namespace FoodDispenserApp.Models
{
    public class SensorData
    {
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
    }
}
