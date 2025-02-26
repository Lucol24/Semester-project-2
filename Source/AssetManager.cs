using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = [];
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];

    public AssetManager()
    {
        LoadProductionUnits(); // Load production unit data from JSON file
        LoadHeatDemand(); // Load heat demand data from CSV file
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

    private void LoadHeatDemand()
    {
        string filePath = "Data/heat_demand.csv";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Error: Heat demand file not found.");
            return;
        }

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }); // "HasHeaderRecord = false" Dont expect headers

        // Skip the first three header lines
        for (int i = 0; i < 3; i++) reader.ReadLine(); // "reader.ReadLine()" reads one line of the file at a time

        while (csv.Read()) // Keep reading the next row until there are no more rows
        {
            winterHeatDemands.Add(new HeatDemand
            {
                // TimeFrom = csv.GetField<DateTime>(0),
            });

            summerHeatDemands.Add(new HeatDemand
            {
                
            });
        }
    }

    /// <summary>
    /// Displays the loaded data in the Console.
    /// </summary>
    public void DisplayData()
    {
        // JSON data - Production Units
        Console.WriteLine("\n--- Production Units ---");
        foreach (var unit in productionUnits)
        {
            Console.WriteLine($"Name: {unit.Name}");
            Console.WriteLine($"  Max Heat: {unit.MaxHeat} MW");
            Console.WriteLine($"  Max Electricity: {(unit.MaxElectricity.HasValue ? unit.MaxElectricity + " MW" : "N/A")}");
            Console.WriteLine($"  Production Costs: {unit.ProductionCosts} DKK/MWh");
            Console.WriteLine($"  CO2 Emissions: {unit.CO2Emissions} kg/MWh");
            Console.WriteLine($"  Fuel Consumption: {unit.FuelConsumption} kg/MWh");
            Console.WriteLine("----------------------------------");
        }

        // CSV data - Winter/Summer Heat Demands
        Console.WriteLine("\n--- ❄️  Winter Heat Demand ❄️  ---\n");
        
            Console.WriteLine($"Print winter demand data");
        

        Console.WriteLine("\n\n--- ☀️  Summer Heat Demand ☀️  ---\n");
        
            Console.WriteLine($"Print winter demand data");   
    }
}

/// <summary>
/// Stores JSON data about the production units (Boilers, Motor and Pump)
/// </summary>
public class ProductionUnit
{
    public string? Name { get; set; } = null; // Nullable string
    public double? MaxHeat { get; set; } = null; // Nullable double
    public double? MaxElectricity { get; set; } = null;
    public double? ProductionCosts { get; set; } = null;
    public double? CO2Emissions { get; set; } = null;
    public double? FuelConsumption { get; set; } = null;
}

/// <summary>
/// Stores winter and summer heat demand records from the CSV
/// </summary>
public class HeatDemand
{
    public DateTime TimeFrom { get; set; } 
    public DateTime TimeTo { get; set; }
    public double Heat { get; set; }
    public double ElectricityPrice { get; set; }
}