
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Compilador2
{
    /// <summary>
    /// Convertidor de valor para Avalonia XAML.
    /// Transforma una ObservableCollection&lt;string&gt; en un string de lineas separadas
    /// por salto de linea, para mostrarse en un TextBox de solo lectura.
    /// </summary>
    public class OutputListConverter : IValueConverter
    {
        /// <summary>Concatena todos los elementos de la coleccion con saltos de linea.</summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<string> outputList)
            {
                return string.Join(Environment.NewLine, outputList);
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}