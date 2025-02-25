using FoodDispenserApp.Models;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FoodDispenserApp.Converters
{
    public class HorarioToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Horario horario)
            {
                return $"{horario.Hora:00}:{horario.Minuto:00} - Duración: {horario.Duracion} min";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}