using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FoodDispenserApp.Models
{
    public class HorariosResponse
    {
        [JsonPropertyName("horarios")]
        public List<Horario> Horarios { get; set; } = new();
    }
}
