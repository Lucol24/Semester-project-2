using System.Globalization;
using System.Text.Json;
using CsvHelper;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = new();

    private List<HeatDemand> WinterHeatDemand = new();

    private List<HeatDemand> SummerHeatDemand = new();

    public AssetManager()
    {
        LoadProductionUnits();
        LoadHeatDemand();
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
        using var readerS = new StreamReader("Data/SummerHeatDemand.csv");
        using var csvS = new CsvReader(readerS, CultureInfo.InvariantCulture);

        var heatDemandDataS = csvS.GetRecords<HeatDemand>();

        foreach(var h in heatDemandDataS)
        {
            Console.WriteLine($"{h.TimeFrom,-10}{h.TimeTo,-10}{h.Heat,-10}{h.ElectricityPrice,-10}");
        }

        Console.WriteLine();

        using var readerW = new StreamReader("Data/WinterHeatDemand.csv");
        using var csvW = new CsvReader(readerW, CultureInfo.InvariantCulture);

        var heatDemandDataW = csvW.GetRecords<HeatDemand>();

        foreach(var h in heatDemandDataW)
        {
            Console.WriteLine($"{h.TimeFrom,-10}{h.TimeTo,-10}{h.Heat,-10}{h.ElectricityPrice,-10}");
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

        Console.WriteLine("Winter Data");
        foreach(var demand in WinterHeatDemand)
        {
            Console.WriteLine($"  Time From: {demand.TimeFrom}");
            Console.WriteLine($"  Time To: {demand.TimeTo}");
            Console.WriteLine($"  Heat Demand: {demand.Heat}");
            Console.WriteLine($"  Electricity Price: {demand.ElectricityPrice}");
            Console.WriteLine("----------------------------------");
        }

        Console.WriteLine("Summer Data");
        foreach(var demand in SummerHeatDemand)
        {
            Console.WriteLine($"  Time From: {demand.TimeFrom}");
            Console.WriteLine($"  Time To: {demand.TimeTo}");
            Console.WriteLine($"  Heat Demand: {demand.Heat}");
            Console.WriteLine($"  Electricity Price: {demand.ElectricityPrice}");
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

public class HeatDemand
{
    public DateTime TimeFrom { get; set; } 
    public DateTime TimeTo { get; set; }
    public double Heat { get; set; }
    public double ElectricityPrice { get; set; }
}