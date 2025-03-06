using System.Text.Json;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = [];
    
    public AssetManager()
    {
        LoadProductionUnits(); // Load production unit data from JSON file
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void LoadProductionUnits()
    {
        string filePath = "Data/production_units.json";
        
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? [];
            }
            else
            {
                LogError("Error: Production units file not found.");
            }
        }
        catch (IOException ex)
        {
            LogError($"I/O Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            LogError($"JSON Parsing Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError($"Unexpected Error: {ex.Message}");
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