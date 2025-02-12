using System.Text.Json.Serialization;

namespace FoodDispenserApp.Models
{
    public class Horario
    {
        [JsonPropertyName("Hora")]
        public int Hora { get; set; }

        [JsonPropertyName("Minuto")]
        public int Minuto { get; set; }

        [JsonPropertyName("Duracion")]
        public int Duracion { get; set; }
    }
}

