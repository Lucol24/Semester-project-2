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
        // Arrange: Setting the work directory and file path
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\..\");
        _filePath = Path.GetFullPath("DanfossHeating/Data/production_units.json");

        // Act: Check file existence
        _fileExists = File.Exists(_filePath);

        // Assert/Logging: Report if file is missing
        if (!_fileExists)
        {
            Console.WriteLine("||-> Production Units JSON file is missing!");
        }
    }

    /// <summary>
    /// Ensures that the JSON file exists.
    /// </summary>
    [Fact]
    public void File_Exists_WhenJsonFileIsPresent()
    {
        // Arrange: Setting the work directory and file path
        //Arrangement is done in the constructor

        // Act: Check if the file exists
        // Act is done in the constructor

        // Assert: Ensure the file exists
        Console.WriteLine("> Checking if the JSON file exists...");
        Assert.True(_fileExists, "||-> Production Units JSON file is missing!");
        Console.WriteLine("||-> JSON file exists");
    }

    /// <summary>
    /// AssetManager handles a JSON file with data (edge case), check if it has any data (succeed).
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_HandlesNonEmptyJsonFile()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");
        // Edge Case: Simulating a JSON file with data to test how AssetManager behaves when data is present.
        
        // Act: Create a new AssetManager instance
        var assetManager = new AssetManager();

        // Use reflection to access the private productionUnits field
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        // Assert: Ensure the productionUnits is not null and not empty
        Console.WriteLine("> Checking that productionUnits JSON is empty (Edge case)...");

        Assert.True(productionUnits?.Count > 0, "||-> Production units are empty.");
        Console.WriteLine("||-> The JSON file contains data");

        if (productionUnits != null && productionUnits.Count > 0)
        {
            Console.WriteLine("\n--- Production Units ---");
            foreach (var unit in productionUnits)
            {
                Console.WriteLine($"Name: {unit.Name ?? "N/A"}");
                Console.WriteLine($"  Max Heat: {unit.MaxHeat}");
                Console.WriteLine($"  Max Electricity: {unit.MaxElectricity + " MW"}");
                Console.WriteLine($"  Production Costs: {unit.ProductionCosts + " DKK/MWh"}");
                Console.WriteLine($"  CO2 Emissions: {unit.CO2Emissions + " kg/MWh"}");
                Console.WriteLine($"  Fuel Consumption: {unit.FuelConsumption + " kg/MWh"}");
                Console.WriteLine("----------------------------------");
            }
        }
    }

    /// <summary>
    /// Verifies the correct number of production units are loaded from the JSON file.
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_ChecksCorrectNumber()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");

        // Arrange: Create a new AssetManager instance
        var assetManager = new AssetManager();
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act: Access the productionUnits list from the AssetManager instance
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        // Assert: Ensure the number of production units matches the expected count
        int expectedCount = 5;
        Console.WriteLine($"> Checking if the number of production units is {expectedCount}...");
        Assert.Equal(expectedCount, productionUnits?.Count);
        Console.WriteLine($"||-> Number of production units: {productionUnits?.Count} (Expected: {expectedCount})");
    }

    /// <summary>
    /// Verifies that the MaxHeat property is correctly populated for each production unit.
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_ValidatesMaxHeatIsPresent()
    {
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");
        // Arrange: Create a new AssetManager instance
        var assetManager = new AssetManager();
        
        // Use reflection to access the private productionUnits field
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        // Act: Check that MaxHeat is not null or default for any production unit
        bool allMaxHeatValid = true;
        int validMaxHeatCount = 0;

        Console.WriteLine("> Checking that MaxHeat is present for all production units...");
        if (productionUnits != null)
        {
            foreach (var unit in productionUnits)
            {
                if (unit.MaxHeat == 0)
                {
                    allMaxHeatValid = false;
                    Console.WriteLine($"||-> Production unit '{unit.Name}' has an invalid MaxHeat value.");
                }
                else
                {
                    validMaxHeatCount++;
                }
            }
        }

        // Assert: Ensure that all production units have a valid MaxHeat value
        Assert.True(allMaxHeatValid, "||-> At least one production unit has an invalid MaxHeat value.");

        // Print message if all MaxHeat values are valid
        if (validMaxHeatCount == productionUnits?.Count)
        {
            Console.WriteLine($"||-> MaxHeat value is present in all {validMaxHeatCount} production units.");
        }
    }
}
