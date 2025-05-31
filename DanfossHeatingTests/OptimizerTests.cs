using DanfossHeating;

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

        /// <summary>
        /// This test verifies that the optimizer selects the correct first machine when optimizing heat production.
        /// It checks that the first machine's name in the optimized schedule matches the expected value ("GM1").
        /// </summary>
        [Fact]
        public void OptimizeHeatProduction_ShouldSelectFirstMachineAsExpected()
        {
            try
            {
                // Arrange
                Console.WriteLine("\n==> [OPT] Testing first machine selection");
                
                SourceDataManager sourceDataManager = new SourceDataManager();
                AssetManager assetManager = new AssetManager();
                ResultDataManager resultDataManager = new ResultDataManager(_filePath);
                Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

                // Act
                var dataOptimized = optimizer.OptimizeHeatProduction("winter", OptimizationCriteria.Cost, true);
                
                // Assert
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

        /// <summary>
        /// This test verifies that the total heat production produced by the optimizer matches the total heat demand for the given season.
        /// It checks that the sum of the heat produced is equal to the total heat demand within a tolerance of 2 decimal places.
        /// </summary>
        [Fact]
        public void OptimizeHeatProduction_ShouldMatchTotalHeatDemand()
        {
            // Arrange
            Console.WriteLine("\n==> [OPT] Testing heat production matches demand");

            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            // Act
            string season = "winter";
            var heatDemands = sourceDataManager.GetWinterHeatDemands();
            double totalHeatDemand = heatDemands.Sum(d => d.Heat);
            var dataOptimized = optimizer.OptimizeHeatProduction(season, OptimizationCriteria.Cost, false);
            double totalHeatProduced = dataOptimized.Sum(schedule => schedule.Schedule.Sum(entry => entry.HeatProduced));

            // Assert
            Assert.Equal(totalHeatDemand, totalHeatProduced, 2);
            Console.WriteLine($"|> Test complete - Heat production ({totalHeatProduced:F2} MW) matches demand ({totalHeatDemand:F2} MW)\n");
        }

        /// <summary>
        /// This test verifies that the optimizer sorts the production units based on their production costs in ascending order.
        /// It checks that the units are correctly sorted and compares the sorted list with the expected order based on their production costs.
        /// </summary>
        [Fact]
        public void OptimizeHeatProduction_ShouldSortUnitsByCost()
        {
            // Arrange
            Console.WriteLine("\n==> [OPT] Testing unit cost-based sorting");

            SourceDataManager sourceDataManager = new SourceDataManager();
            AssetManager assetManager = new AssetManager();
            ResultDataManager resultDataManager = new ResultDataManager(_filePath);
            Optimizer optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

            // Act
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

            // Assert
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
