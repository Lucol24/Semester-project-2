using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DanfossHeating.Converters;

public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = "true";
    public string FalseValue { get; set; } = "false";
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        
        return FalseValue;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue.Equals(TrueValue, StringComparison.OrdinalIgnoreCase);
        }
        
        return false;
    }
}