using System.Text.Json;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = new();

    public AssetManager()
    {
        LoadProductionUnits();
    }

    private void LoadProductionUnits()
    {
        string filePath = "Data/production_units.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? new();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error: Production units file not found.");
            Console.ResetColor();
        }
    }

    public void DisplayData()
    {
        Console.WriteLine("\n--- Production Units ---");
        foreach (var unit in productionUnits)
        {
            Console.WriteLine($"Name: {unit.Name ?? "N/A"}");
            Console.WriteLine($"  Max Heat: {(unit.MaxHeat.HasValue ? unit.MaxHeat + " MW" : "N/A")}");
            Console.WriteLine($"  Max Electricity: {(unit.MaxElectricity.HasValue ? unit.MaxElectricity + " MW" : "N/A")}");
            Console.WriteLine($"  Production Costs: {(unit.ProductionCosts.HasValue ? unit.ProductionCosts + " DKK/MWh" : "N/A")}");
            Console.WriteLine($"  CO2 Emissions: {(unit.CO2Emissions.HasValue ? unit.CO2Emissions + " kg/MWh" : "N/A")}");
            Console.WriteLine($"  Fuel Consumption: {(unit.FuelConsumption.HasValue ? unit.FuelConsumption + " kg/MWh" : "N/A")}");
            Console.WriteLine("----------------------------------");
        }
    }
}

public class ProductionUnit
{
    public string? Name { get; set; } = null; // Nullable string
    public double? MaxHeat { get; set; } = null; // Nullable double
    public double? MaxElectricity { get; set; } = null;
    public double? ProductionCosts { get; set; } = null;
    public double? CO2Emissions { get; set; } = null;
    public double? FuelConsumption { get; set; } = null;
}