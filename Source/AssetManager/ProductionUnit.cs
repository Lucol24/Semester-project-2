namespace DanfossHeating;

/// <summary>
/// Stores JSON data about the production units (Boilers, Motor and Pump)
/// </summary>
public class ProductionUnit
{
    public string? Name { get; set; } = null; // Nullable string
    public required double MaxHeat { get; set; } 
    public double? MaxElectricity { get; set; } = null;
    public double? ProductionCosts { get; set; } = null;
    public double? CO2Emissions { get; set; } = null;
    public double? FuelConsumption { get; set; } = null;
}