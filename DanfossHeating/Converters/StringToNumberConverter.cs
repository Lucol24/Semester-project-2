using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace DanfossHeating.Converters
{
    public class StringToNumberConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // When converting from number to string for display
            if (value == null)
                return "0"; // Display null as "0"

            // Format the number consistently, e.g., using InvariantCulture
            if (value is double d)
                return d.ToString(CultureInfo.InvariantCulture);
            if (value is int i)
                return i.ToString(CultureInfo.InvariantCulture);
            if (value is decimal dec)
                return dec.ToString(CultureInfo.InvariantCulture);

            return value.ToString() ?? "0"; // Fallback
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // When converting from string back to number for the model
            if (value is not string stringValue)
            {
                // If the input isn't even a string, return 0
                return 0.0; // Assuming target is double/decimal
            }

            // Handle empty or whitespace strings explicitly
            if (string.IsNullOrWhiteSpace(stringValue))
                return 0.0; // Treat empty as 0

            // Try to parse as double using InvariantCulture for consistency
            if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                // Check if the target type is int and convert if necessary
                 if (targetType == typeof(int))
                 {
                     return (int)Math.Round(result); // Or handle potential overflow
                 }
                 // Add checks for other numeric types if needed (decimal, float, etc.)
                 // else if (targetType == typeof(decimal)) { ... }

                return result; // Return as double if target is double or compatible
            }

            // If parsing fails, return 0. The LostFocus handler should prevent this state mostly.
            return 0.0;
        }
    }
}
