namespace DanfossHeating;

/// <summary>
/// Stores JSON data about the production units (Boilers, Motor and Pump)
/// </summary>
public class ProductionUnit
{
    public string? Name { get; set; }
    public double? MaxHeat { get; set; }
    public double? MaxElectricity { get; set; }
    public double? ProductionCosts { get; set; }
    public double? CO2Emissions { get; set; }
    public double? FuelConsumption { get; set; }
    public string? ImagePath { get; set; } 
}