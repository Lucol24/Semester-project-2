using Xunit;
using DanfossHeating;
using System.Reflection;

public class SourceDataManagerTests
{
    /// <summary>
    /// Ensures that the CSV file exists.
    /// </summary>
    [Fact]
    public void File_Exists_WhenCSVFileIsPresent()
    {
        // Arrange: Setting the work directory and file path
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\");
        string filePath = Path.GetFullPath("Data/heat_demand.csv");

        // Act: Check if the file exists
        bool fileExists = File.Exists(filePath);

        // Assert: Ensure the file exists
        Console.WriteLine("> Checking if the CSV file exists...");
        Assert.True(fileExists, "||-> Heat Demand CSV file is missing!");
        Console.WriteLine("||-> CSV file exists");
    }


    /// <summary>
    /// Ensures that winter and summer heat demand data exist and are not null.
    /// If data is available, it prints the heat demand details.
    /// </summary>
    [Fact]
    public void LoadHeatDemand_EnsuresWinterAndSummerDataExist()
    {
        // Arrange: Create an instance of SourceDataManager
       var sourceDataManager = new SourceDataManager();
       
        // Act: Retrieve winter and summer heat demand data
        var winterDemands = sourceDataManager.GetWinterHeatDemands();
        var summerDemands = sourceDataManager.GetSummerHeatDemands();
       
        // Assert: Ensure the heat demand lists are not null and not empty
        Console.WriteLine("> Checking if winter heat demand data exists...");
        Assert.NotNull(winterDemands);
        Assert.NotEmpty(winterDemands);
        Console.WriteLine("||-> Winter heat demand data exists and is not empty");

        Console.WriteLine("> Checking if summer heat demand data exists...");
        Assert.NotNull(summerDemands);
        Assert.NotEmpty(summerDemands);
        Console.WriteLine("||-> Summer heat demand data exists and is not empty"); 
    }

    [Fact]
    public void AllHeatDemands_ShouldHaveElectricityPriceGreaterThanZero()
    {
        // Arrange
        var sourceDataManager = new SourceDataManager();
        List<HeatDemand> winterHeatDemands = sourceDataManager.GetWinterHeatDemands();
        List<HeatDemand> summerHeatDemands = sourceDataManager.GetSummerHeatDemands();

        // Act & Assert
        foreach (var demand in winterHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, $"Winter demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }
       
        foreach (var demand in summerHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, $"Summer demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }
        Console.WriteLine("All heat demands have ElectricityPrice greater than zero.");
    }
}