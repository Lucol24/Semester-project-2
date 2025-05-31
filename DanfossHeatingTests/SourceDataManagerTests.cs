using DanfossHeating;

namespace DanfossHeatingTests;

public class SourceDataManagerTests
{
    private readonly string _filePath;
    private readonly bool _fileExists;

    public SourceDataManagerTests()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\..\");
        _filePath = Path.GetFullPath("DanfossHeating/Data/heat_demand.csv");
        _fileExists = File.Exists(_filePath);
    }

    /// <summary>
    /// This test verifies that the Heat Demand CSV file exists and is accessible.
    /// It checks that the _fileExists flag is true, indicating the file is present in the expected location.
    /// </summary>
    [Fact]
    public void File_ShouldExist_WhenHeatDemandCSVFileIsPresent()
    {
        Console.WriteLine("\n==> [SDM] Testing CSV file existence");
        Assert.True(_fileExists, "Heat Demand CSV file is missing!");
        Console.WriteLine("|> Test complete - CSV file exists and is accessible\n");
    }

    /// <summary>
    /// This test verifies that both winter and summer heat demand data exist and are not empty.
    /// It ensures that the data for both seasons can be loaded correctly from the CSV file.
    /// If the Heat Demand CSV file is missing, the test is skipped.
    /// </summary>
    [SkippableFact]
    public void LoadHeatDemand_ShouldEnsureWinterAndSummerDataExist()
    {
        Skip.IfNot(_fileExists, "Skipping test: Heat Demand CSV file is missing!");

        Console.WriteLine("\n==> [SDM] Testing seasonal heat demand data");
        var sourceDataManager = new SourceDataManager();
        var winterDemands = sourceDataManager.GetWinterHeatDemands();
        var summerDemands = sourceDataManager.GetSummerHeatDemands();
       
        Assert.NotNull(winterDemands);
        Assert.NotEmpty(winterDemands);
        Assert.NotNull(summerDemands);
        Assert.NotEmpty(summerDemands);

        Console.WriteLine("|> Test complete - Found valid seasonal data:");
        Console.WriteLine($"    Winter entries: {winterDemands.Count}");
        Console.WriteLine($"    Summer entries: {summerDemands.Count}\n");
    }

    /// <summary>
    /// This test verifies that all heat demand entries for both winter and summer have a valid electricity price greater than zero.
    /// It ensures that the electricity price for each demand entry is positive, as this is a necessary condition for valid data.
    /// If the Heat Demand CSV file is missing, the test is skipped.
    /// </summary>
    [SkippableFact]
    public void LoadHeatDemand_ShouldEnsureElectricityPriceIsGreaterThanZero()
    {
        Skip.IfNot(_fileExists, "Skipping test: Heat Demand CSV file is missing!");

        Console.WriteLine("\n==> [SDM] Testing electricity price validation");
        var sourceDataManager = new SourceDataManager();
        List<HeatDemand> winterHeatDemands = sourceDataManager.GetWinterHeatDemands();
        List<HeatDemand> summerHeatDemands = sourceDataManager.GetSummerHeatDemands();

        foreach (var demand in winterHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, 
                $"Winter demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }
        
        foreach (var demand in summerHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, 
                $"Summer demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }

        Console.WriteLine("|> Test complete - All electricity prices are valid:");
        Console.WriteLine($"    Winter entries checked: {winterHeatDemands.Count}");
        Console.WriteLine($"    Summer entries checked: {summerHeatDemands.Count}\n");
    }
}