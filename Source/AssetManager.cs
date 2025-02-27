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
                TimeFrom = csv.GetField<DateTime>(0),
                TimeTo = csv.GetField<DateTime>(1),
                Heat = csv.GetField<double>(2),
                ElectricityPrice = csv.GetField<double>(3),
            });

            summerHeatDemands.Add(new HeatDemand
            {
                TimeFrom = csv.GetField<DateTime>(5),
                TimeTo = csv.GetField<DateTime>(6),
                Heat = csv.GetField<double>(7),
                ElectricityPrice = csv.GetField<double>(8),
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
            Console.WriteLine($"Name: {unit.Name ?? "N/A"}");
            Console.WriteLine($"  Max Heat: {(unit.MaxHeat.HasValue ? unit.MaxHeat + " MW" : "N/A")}");
            Console.WriteLine($"  Max Electricity: {(unit.MaxElectricity.HasValue ? unit.MaxElectricity + " MW" : "N/A")}");
            Console.WriteLine($"  Production Costs: {(unit.ProductionCosts.HasValue ? unit.ProductionCosts + " DKK/MWh" : "N/A")}");
            Console.WriteLine($"  CO2 Emissions: {(unit.CO2Emissions.HasValue ? unit.CO2Emissions + " kg/MWh" : "N/A")}");
            Console.WriteLine($"  Fuel Consumption: {(unit.FuelConsumption.HasValue ? unit.FuelConsumption + " kg/MWh" : "N/A")}");
            Console.WriteLine("----------------------------------");
        }

        // CSV data - Winter/Summer Heat Demands
        Console.WriteLine("\n--- ❄️  Winter Heat Demand ❄️  ---\n");
        foreach (var demand in winterHeatDemands)
        {
            Console.WriteLine($"Time From: {demand.TimeFrom}");
            Console.WriteLine($"Time To: {demand.TimeTo}");
            Console.WriteLine($"Heat: {demand.Heat} MW");
            Console.WriteLine($"Electricity Price: {demand.ElectricityPrice} DKK/MWh");
            Console.WriteLine("----------------------------------");
        }        
        

        Console.WriteLine("\n\n--- ☀️  Summer Heat Demand ☀️  ---\n");
        foreach (var demand in summerHeatDemands)
        {
            Console.WriteLine($"Time From: {demand.TimeFrom}");
            Console.WriteLine($"Time To: {demand.TimeTo}");
            Console.WriteLine($"Heat: {demand.Heat} MW");
            Console.WriteLine($"Electricity Price: {demand.ElectricityPrice} DKK/MWh");        }        
    }
}

