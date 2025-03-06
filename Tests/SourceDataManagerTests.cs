using Xunit;
using DanfossHeating;
using System.Reflection;

public class SourceDataManagerTests
{
    /// <summary>
    /// Ensures that the CSV file exists.
    /// </summary>
    [Fact] // Mats and Carolina
    public void File_Exists_WhenCSVFileIsPresent()
    {
        // Arrange: Setting the work directory and file path
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\");
        string filePath = Path.GetFullPath("Data/production_units.json");

        // Act: Check if the file exists
        bool fileExists = File.Exists(filePath);

        // Assert: Ensure the file exists
        Console.WriteLine("> Checking if the JSON file exists...");
        Assert.True(fileExists, "||-> Production Units JSON file is missing!");
        Console.WriteLine("||-> JSON file exists");
    }
}