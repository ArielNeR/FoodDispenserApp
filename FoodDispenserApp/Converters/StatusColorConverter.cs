using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace FoodDispenserApp.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Contains("Conectado"))
                    return Color.FromHex("#00FF00"); // Verde para conectado
                if (status.Contains("Error"))
                    return Color.FromHex("#FF0000"); // Rojo para error
                return Color.FromHex("#E5E5E5"); // Blanco Netflix por defecto
            }
            return Color.FromHex("#E5E5E5"); // Fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}