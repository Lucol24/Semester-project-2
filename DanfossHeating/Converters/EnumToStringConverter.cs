using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DanfossHeating.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string stringValue = value.ToString() ?? string.Empty;
            string targetValue = parameter.ToString() ?? string.Empty;

            // Check if we have multiple parameters (using : as separator)
            if (targetValue.Contains(':'))
            {
                string[] parts = targetValue.Split(':');
                if (parts.Length >= 3 && stringValue.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                    return parts[1]; // return the "true" value
                else
                    return parts[2]; // return the "false" value
            }

            // Simple string equality
            return stringValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool isChecked = value is bool bVal && bVal;
            string paramString = parameter.ToString() ?? string.Empty;

            // If this is a RadioButton scenario, we're being asked if the target value
            // should be the checked one
            if (isChecked)
                return paramString;

            return null;
        }
    }
}
