using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using DanfossHeating.ViewModels;

namespace DanfossHeating.Converters;

public class MachineTypeConverter : IValueConverter
{
    private static readonly Dictionary<string, (string Name, string Color)> MachineTypes = new()
    {
        { "GB1", ("Gas Boiler 1", "#FF4D4D") },
        { "GB2", ("Gas Boiler 2", "#FF4D4D") },
        { "OB1", ("Oil Boiler", "#FFA64D") },
        { "GM1", ("Gas Motor", "#4D94FF") },
        { "HP1", ("Heat Pump", "#4DFF88") }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string id && parameter is string type)
        {
            if (MachineTypes.TryGetValue(id, out var info))
            {
                return type.ToLowerInvariant() switch
                {
                    "color" => info.Color,
                    "name" => info.Name,
                    _ => value
                };
            }
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}