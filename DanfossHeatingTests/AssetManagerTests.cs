using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DanfossHeating;
using Xunit;

namespace DanfossHeatingTests;

public class AssetManagerTests
{
    private readonly string _filePath;
    private readonly bool _fileExists;

    public AssetManagerTests()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\..\");
        _filePath = Path.GetFullPath("DanfossHeating/Data/production_units.json");
        _fileExists = File.Exists(_filePath);
    }

    [Fact]
    public void File_Exists_WhenJsonFileIsPresent()
    {
        Console.WriteLine("\n==> [AM] Testing JSON file existence");
        Assert.True(_fileExists, "Production Units JSON file is missing!");
        Console.WriteLine("|> Test complete - JSON file exists and is accessible\n");
    }

    [SkippableFact]
    public void LoadProductionUnits_HandlesNonEmptyJsonFile()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");
        
        Console.WriteLine("\n==> [AM] Testing production units data loading");
        var assetManager = new AssetManager();
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        Assert.True(productionUnits?.Count > 0, "Production units list is empty.");
        Console.WriteLine("|> Test complete - Production units found:");
        foreach (var unit in productionUnits!)
        {
            Console.WriteLine($"    Unit: {unit.Name ?? "N/A"}");
            Console.WriteLine($"        Heat: {unit.MaxHeat} MW");
            Console.WriteLine($"        Electricity: {unit.MaxElectricity} MW");
            Console.WriteLine($"        Costs: {unit.ProductionCosts} DKK/MWh");
            Console.WriteLine($"        CO2: {unit.CO2Emissions} kg/MWh");
            Console.WriteLine($"        Fuel: {unit.FuelConsumption} kg/MWh");
        }
        Console.WriteLine();
    }

    [SkippableFact]
    public void LoadProductionUnits_ChecksCorrectNumber()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");

        Console.WriteLine("\n==> [AM] Testing number of production units");
        var assetManager = new AssetManager();
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        int expectedCount = 5;
        Assert.Equal(expectedCount, productionUnits?.Count);
        Console.WriteLine($"|> Test complete - Found {productionUnits?.Count} units (Expected: {expectedCount})\n");
    }

    [SkippableFact]
    public void LoadProductionUnits_ValidatesMaxHeatIsPresent()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");

        Console.WriteLine("\n==> [AM] Testing MaxHeat values validation");
        var assetManager = new AssetManager();
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        bool allMaxHeatValid = true;
        int validMaxHeatCount = 0;

        if (productionUnits != null)
        {
            foreach (var unit in productionUnits)
            {
                if (unit.MaxHeat == 0)
                {
                    allMaxHeatValid = false;
                    Console.WriteLine($"|> Warning: Unit '{unit.Name}' has MaxHeat = 0");
                }
                else
                {
                    validMaxHeatCount++;
                }
            }
        }

        Assert.True(allMaxHeatValid, "One or more production units have invalid MaxHeat values.");
        Console.WriteLine($"|> Test complete - All {validMaxHeatCount} units have valid MaxHeat values\n");
    }
}
