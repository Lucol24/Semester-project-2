using System.Reflection;
using DanfossHeating;

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

    /// <summary>
    /// This test verifies that the JSON file exists and is accessible.
    /// It checks that the _fileExists flag is true, indicating that the file is present in the expected location.
    /// </summary>
    [Fact]
    public void File_ShouldExist_WhenJsonFileIsPresent()
    {
        Console.WriteLine("\n==> [AM] Testing JSON file existence");
        Assert.True(_fileExists, "Production Units JSON file is missing!");
        Console.WriteLine("|> Test complete - JSON file exists and is accessible\n");
    }

    /// <summary>
    /// This test verifies that the application correctly loads production units data from the JSON file,
    /// provided that the file exists. The test checks that the list of production units is non-empty.
    /// If the production units file is missing, the test is skipped.
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_ShouldHandleNonEmptyJsonFile()
    {
        // Arrange
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");
        
        Console.WriteLine("\n==> [AM] Testing production units data loading");
        var assetManager = new AssetManager();

        // Act
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        // Assert
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

    /// <summary>
    /// This test verifies that the correct number of production units are loaded from the JSON file.
    /// It compares the loaded count of production units with the expected count (5).
    /// If the production units file is missing, the test is skipped.
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_ShouldHaveCorrectNumberOfUnits()
    {
        // Arrange
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");

        Console.WriteLine("\n==> [AM] Testing number of production units");
        var assetManager = new AssetManager();

        // Act
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        int expectedCount = 5;

        // Assert
        Assert.Equal(expectedCount, productionUnits?.Count);
        Console.WriteLine($"|> Test complete - Found {productionUnits?.Count} units (Expected: {expectedCount})\n");
    }
    
    /// <summary>
    /// This test verifies that all production units have valid MaxHeat values. 
    /// It ensures that no unit has a MaxHeat value of 0, which is considered invalid.
    /// If the production units file is missing, the test is skipped.
    /// </summary>
    [SkippableFact]
    public void LoadProductionUnits_ShouldValidateMaxHeatValues()
    {
        // Arrange
        Skip.IfNot(_fileExists, "Skipping test: production Units JSON file is missing!");

        Console.WriteLine("\n==> [AM] Testing MaxHeat values validation");
        var assetManager = new AssetManager();
        var productionUnitsField = typeof(AssetManager)
            .GetField("productionUnits", BindingFlags.NonPublic | BindingFlags.Instance);
        var productionUnits = productionUnitsField?.GetValue(assetManager) as List<ProductionUnit>;

        // Act
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

        // Assert
        Assert.True(allMaxHeatValid, "One or more production units have invalid MaxHeat values.");
        Console.WriteLine($"|> Test complete - All {validMaxHeatCount} units have valid MaxHeat values\n");
    }
}
