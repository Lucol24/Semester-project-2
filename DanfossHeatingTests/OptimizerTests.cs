using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DanfossHeating;
using Xunit;

namespace DanfossHeatingTests
{
    public class OptimizerTests : IDisposable // to delete the data folder after tests
    {
        private readonly string _dataFolderPath;
        private readonly string _filePath;

        public OptimizerTests()
        {
            string projectDirectory = GetTestProjectRoot();
            _dataFolderPath = Path.Combine(projectDirectory, "Data");
            _filePath = Path.Combine(_dataFolderPath, "result_data.csv");
            Directory.CreateDirectory(_dataFolderPath);
            File.WriteAllText(_filePath, "Default,CSV,Content\n");
        }

        [Fact]
        public void First_Used_Machine()
        {
            try
            {
                Console.WriteLine("\n==> [OPT] Testing first machine selection");
                
                SourceDataManager sourceDataManager = new SourceDataManager();
                AssetManager assetManager = new AssetManager();
                ResultDataManager resultDataManager = new ResultDataManager(_filePath);
                Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

                var dataOptimized = optimizer.OptimizeHeatProduction("winter", OptimizationCriteria.Cost, true);
                
                Assert.NotNull(dataOptimized);
                Assert.NotEmpty(dataOptimized);
                Assert.NotEmpty(dataOptimized[0].Schedule);
                
                var firstMachine = dataOptimized[0].Schedule[0].UnitName;
                Assert.Equal("GM1", firstMachine);
                Console.WriteLine($"|> Test complete - First machine is {firstMachine} as expected\n");
            }
            finally
            {
                Dispose();
            }
        }

        [Fact]
        public void Total_Heat_Production_Matches_Demand()
        {
            Console.WriteLine("\n==> [OPT] Testing heat production matches demand");

            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            string season = "winter";
            var heatDemands = sourceDataManager.GetWinterHeatDemands();
            double totalHeatDemand = heatDemands.Sum(d => d.Heat);
            var dataOptimized = optimizer.OptimizeHeatProduction(season, OptimizationCriteria.Cost, false);
            double totalHeatProduced = dataOptimized.Sum(schedule => schedule.Schedule.Sum(entry => entry.HeatProduced));

            Assert.Equal(totalHeatDemand, totalHeatProduced, 2);
            Console.WriteLine($"|> Test complete - Heat production ({totalHeatProduced:F2} MW) matches demand ({totalHeatDemand:F2} MW)\n");
        }

        [Fact]
        public void Units_Are_Sorted_By_Cost()
        {
            Console.WriteLine("\n==> [OPT] Testing unit cost-based sorting");

            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            var largeHeatDemand = sourceDataManager.GetSummerHeatDemands();
            if (largeHeatDemand.Count > 0)
            {
                largeHeatDemand[0].Heat = 15.0;
            }

            var dataOptimized = optimizer.OptimizeHeatProduction("summer", OptimizationCriteria.Cost, false);
            var sortedUnits = dataOptimized.Select(schedule => schedule.UnitName).ToList();

            var expectedOrder = assetManager.GetProductionUnits()
                .Where(u => u.Name != null && (u.Name == "GB1" || u.Name == "GB2" || u.Name == "OB1"))
                .OrderBy(u => u.ProductionCosts)
                .Select(u => u.Name!)
                .ToList();

            Assert.Equal(expectedOrder, sortedUnits);
            
            Console.WriteLine("|> Test complete - Units are correctly sorted by cost:");
            for (int i = 0; i < sortedUnits.Count; i++)
            {
                var unit = assetManager.GetProductionUnits().First(u => u.Name == sortedUnits[i]);
                Console.WriteLine($"    {i + 1}. {unit.Name} - {unit.ProductionCosts} DKK/MWh");
            }
            Console.WriteLine();
        }

        private string GetTestProjectRoot()
        {
            string? current = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(current) && Path.GetFileName(current) != "DanfossHeatingTests")
            {
                current = Directory.GetParent(current)?.FullName;
            }
            if (string.IsNullOrEmpty(current))
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
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }
    }
}
