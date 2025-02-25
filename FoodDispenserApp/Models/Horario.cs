namespace FoodDispenserApp.Models
{
    public class Horario
    {
        private int _hora;
        public int Hora
        {
            get => _hora;
            set => _hora = value is >= 0 and <= 23 ? value : throw new ArgumentException("Hora debe estar entre 0 y 23.");
        }

        private int _minuto;
        public int Minuto
        {
            get => _minuto;
            set => _minuto = value is >= 0 and <= 59 ? value : throw new ArgumentException("Minuto debe estar entre 0 y 59.");
        }

        private int _duracion;
        public int Duracion
        {
            get => _duracion;
            set => _duracion = value > 0 ? value : throw new ArgumentException("Duración debe ser mayor a 0.");
        }
    }
}