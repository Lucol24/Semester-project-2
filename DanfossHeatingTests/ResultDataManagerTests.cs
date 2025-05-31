using DanfossHeating;

namespace DanfossHeatingTests;

public class ResultDataManagerTests
{
    private readonly string _filePath;
    private readonly bool _fileExists;

    public ResultDataManagerTests()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\..\");
        _filePath = Path.GetFullPath("DanfossHeating/Data/result_data.csv");
        _fileExists = File.Exists(_filePath);
    }

    /// <summary>
    /// This test verifies that the CSV file exists and is accessible.
    /// It checks that the _fileExists flag is true, indicating the file is present in the expected location.
    /// </summary>
    [Fact]
    public void File_ShouldExist_WhenCSVFileIsPresent()
    {
        Console.WriteLine("\n==> [RDM] Testing CSV file existence");
        Assert.True(_fileExists, "Result Data CSV file is missing!");
        Console.WriteLine("|> Test complete - CSV file exists and is accessible\n");
    }

    /// <summary>
    /// This test verifies that new data is successfully saved to the CSV file.
    /// It checks that the file is created, and that the data saved to the file matches the expected data.
    /// </summary>
    [Fact]
    public void SaveResults_ShouldSaveDataToCSVFile()
    {
        // Arrange
        Console.WriteLine("\n==> [RDM] Testing if new data is saved to the CSV file");

        // Ensure the Data folder exists
        string? dataFolderPath = Path.GetDirectoryName(_filePath);
        if (dataFolderPath == null)
        {
            throw new InvalidOperationException("Unable to determine the data folder path.");
        }
        
        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        // Create a new instance of ResultDataManager
        ResultDataManager resultDataManager = new(_filePath);

        // Prepare test data
        var testResults = new List<ResultEntry>
        {
            new ResultEntry("Unit1", DateTime.Now, 100.0, 50.0, 200.0, 10.0, 5.0),
            new ResultEntry("Unit2", DateTime.Now, 150.0, 75.0, 300.0, 15.0, 7.5)
        };

        // Act
        resultDataManager.SaveResults(testResults);

        // Assert
        Assert.True(File.Exists(_filePath), "CSV file was not created!");

        // Verify the contents of the file
        var savedResults = resultDataManager.LoadResults();
        Assert.Equal(testResults.Count, savedResults.Count);
        for (int i = 0; i < testResults.Count; i++)
        {
            Assert.Equal(testResults[i].UnitName, savedResults[i].UnitName);
            Assert.Equal(testResults[i].HeatProduced, savedResults[i].HeatProduced);
            Assert.Equal(testResults[i].ElectricityProduced, savedResults[i].ElectricityProduced);
            Assert.Equal(testResults[i].ProductionCost, savedResults[i].ProductionCost);
            Assert.Equal(testResults[i].FuelConsumption, savedResults[i].FuelConsumption);
            Assert.Equal(testResults[i].CO2Emissions, savedResults[i].CO2Emissions);
        }

        Console.WriteLine("|> Test complete - New data was successfully saved to the CSV file\n");
    }
}