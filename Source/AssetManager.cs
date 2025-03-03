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

    private void LogError(string message) // Method for printing red error messages
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
                productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? new();
            }
            else
            {
                LogError("Error: Production units file not found.");
            }
        }
        catch (IOException ex) // Catches errors related to file reading/writing
        {
            LogError($"I/O Error: {ex.Message}");
        }
        catch (JsonException ex) // Catches errors when JSON format is incorrect or corrupted
        {
            LogError($"JSON Parsing Error: {ex.Message}");
        }
        catch (Exception ex) // Catches any other unexpected errors
        {
            LogError($"Unexpected Error: {ex.Message}");
        }
    }

    private void LoadHeatDemand()
    {
        string filePath = "Data/heat_demand.csv";
        
        try
        {
            if (!File.Exists(filePath))
            {
                LogError("Error: Heat demand file not found.");
                return;
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false });

            // Skip the first three header lines
            for (int i = 0; i < 3; i++) 
            {
                if (reader.ReadLine() == null)
                {
                    throw new InvalidDataException("Error: Unexpected end of file while skipping header lines.");
                }
            }

            while (csv.Read()) // Keep reading the next row until there are no more rows
            {
                try
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
                catch (CsvHelperException ex) // Catches errors when parsing CSV rows -> like wrong alues or missing types
                {
                    LogError($"CSV Parsing Error at row {csv.Context.Row}: {ex.Message}");
                }
            }
        }
        catch (FileNotFoundException ex) // Catches errors if file is missing
        {
            LogError($"File Not Found: {ex.Message}");
        }
        catch (IOException ex) // Catches errors related to file reading/writing
        {
            LogError($"I/O Error: {ex.Message}");
        }
        catch (CsvHelperException ex) // Catches CSV parsing errors that occur outside of the row-level try-catch
        {
            LogError($"CSV Parsing Error: {ex.Message}");
        }
        catch (InvalidDataException ex) // Catches manually thrown errors when CSV structure is unexpected
        {
            LogError($"Invalid Data: {ex.Message}");
        }
        catch (Exception ex) // Catches any other unexpected errors
        {
            LogError($"Unexpected Error: {ex.Message}");
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
        Console.WriteLine("\n---❄️  Winter Heat Demand ❄️  ---\n");
        foreach (var demand in winterHeatDemands)
        {
            Console.WriteLine($"From {demand.TimeFrom} to {demand.TimeTo}, Heat: {demand.Heat} MWh, Electricity Price: {demand.ElectricityPrice} DKK/MWh");
        }        
        
        Console.WriteLine("\n\n--- ☀️  Summer Heat Demand ☀️  ---\n");
        foreach (var demand in summerHeatDemands)
        {
            Console.WriteLine($"From {demand.TimeFrom} to {demand.TimeTo}, Heat: {demand.Heat} MWh, Electricity Price: {demand.ElectricityPrice} DKK/MWh");
        }        
    }
}
