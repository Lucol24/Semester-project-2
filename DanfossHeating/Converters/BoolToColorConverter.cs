using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DanfossHeating.Converters;
public class BoolToColorConverter : IValueConverter
{
    public object TrueValue { get; set; } = Brushes.White;
    public object FalseValue { get; set; } = Brushes.Black;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // If we have a parameter with color values in format "TrueColor:FalseColor"
            if (parameter is string colorParam && colorParam.Contains(':'))
            {
                var colors = colorParam.Split(':');
                if (colors.Length == 2)
                {
                    string selectedColor = boolValue ? colors[0] : colors[1];
                    
                    // Check if we're converting to a color or brush
                    if (targetType == typeof(Color) || targetType == typeof(Color?))
                    {
                        return Color.Parse(selectedColor);
                    }
                    else if (targetType == typeof(IBrush) || targetType == typeof(ISolidColorBrush))
                    {
                        return new SolidColorBrush(Color.Parse(selectedColor));
                    }
                }
            }
            
            return boolValue ? TrueValue : FalseValue;
        }
        
        return FalseValue;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(TrueValue);
    }
}