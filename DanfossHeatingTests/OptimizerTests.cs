using System;
using System.IO;
using System.Reflection;
using DanfossHeating;
using Xunit;

namespace DanfossHeatingTests
{
    public class OptimizerTests : IDisposable
    {
        private readonly string _dataFolderPath;
        private readonly string _filePath;

        public OptimizerTests()
        {
            // Get the path to the DanfossHeatingTests project directory
            string projectDirectory = GetTestProjectRoot();
            _dataFolderPath = Path.Combine(projectDirectory, "Data");
            _filePath = Path.Combine(_dataFolderPath, "result_data.csv");

            // Ensure the Data folder exists
            Directory.CreateDirectory(_dataFolderPath);
            Console.WriteLine($"||-> Ensured Data folder exists at {_dataFolderPath}");

            // Ensure the file exists
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"||-> Creating missing CSV file at {_filePath}");
                File.WriteAllText(_filePath, "Default,CSV,Content\n");
            }
        }

        [Fact]
        public void First_Used_Machine()
        {
            try
            {
                // Arrange
                SourceDataManager sourceDataManager = new SourceDataManager();
                AssetManager assetManager = new AssetManager();
                ResultDataManager resultDataManager = new ResultDataManager(_filePath);
                Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

                // Act
                var dataOptimized = optimizer.OptimizeHeatProduction("winter", OptimizationCriteria.Cost, true);
                var dataToCheck = dataOptimized[0].Schedule[0].UnitName;
                bool condition = dataToCheck == "GM1";

                // Assert
                Console.WriteLine("> Checking if the first used machine is the right one ...");
                Assert.True(condition, "||-> GM1 is not used as the first machine!");
                Console.WriteLine("||-> GM1 is used as the first machine!");
            }
            finally
            {
                Dispose();
            }
        }

        private string GetTestProjectRoot()
        {
            string current = AppDomain.CurrentDomain.BaseDirectory;

            while (current != null && Path.GetFileName(current) != "DanfossHeatingTests")
            {
                current = Directory.GetParent(current)?.FullName;
            }

            if (current == null)
            {
                throw new DirectoryNotFoundException("Could not locate DanfossHeatingTests project directory.");
            }

            return current;
        }



        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_dataFolderPath))
                {
                    Directory.Delete(_dataFolderPath, true);
                    Console.WriteLine($"||-> Data folder deleted at {_dataFolderPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"||-> Cleanup error: {ex.Message}");
            }
        }

        [Fact]
        public void Total_Heat_Production_Matches_Demand()
        {
            // Arrange
            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            string season = "winter";
            var heatDemands = sourceDataManager.GetWinterHeatDemands();
            double totalHeatDemand = heatDemands.Sum(d => d.Heat);

            // Act
            var dataOptimized = optimizer.OptimizeHeatProduction(season, OptimizationCriteria.Cost, false);
            double totalHeatProduced = dataOptimized.Sum(schedule => schedule.Schedule.Sum(entry => entry.HeatProduced));

            // Assert
            Console.WriteLine($"> Total Heat Demand: {totalHeatDemand}, Total Heat Produced: {totalHeatProduced}");
            Assert.Equal(totalHeatDemand, totalHeatProduced, precision: 2); // Allow small rounding differences
        }

        [Fact]
        public void Units_Are_Sorted_By_Cost()
        {
            // Arrange
            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            string season = "summer";

            // Act
            var dataOptimized = optimizer.OptimizeHeatProduction(season, OptimizationCriteria.Cost, false);
            var sortedUnits = dataOptimized.Select(schedule => schedule.UnitName).ToList();

            // Assert
            Console.WriteLine("> Verifying units are sorted by cost...");
            Assert.True(sortedUnits.SequenceEqual(sortedUnits.OrderBy(unitName => assetManager.GetProductionUnits()
                .First(u => u.Name == unitName).ProductionCosts)), "Units are not sorted by cost!");
            Console.WriteLine("||-> Units are correctly sorted by cost.");
        }
    }
}
