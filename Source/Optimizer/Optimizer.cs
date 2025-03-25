namespace DanfossHeating;

/// <summary>
/// The Optimizer class is responsible for calculating the optimal heat production schedule.
/// It determines which production units should be used based on cost and efficiency.
/// </summary>
public class Optimizer
{
    // These are dependencies required for the optimization process.
    private readonly AssetManager _assetManager; // Provides access to production units (boilers, motors, etc.).
    private readonly SourceDataManager _sourceDataManager; // Provides heat demand data (csv information).
    private readonly ResultDataManager _resultDataManager; // Stores the optimization results.
    
    /// <summary>
    /// Constructor to initialize the Optimizer with its required dependencies.
    /// </summary>
    public Optimizer(AssetManager assetManager, SourceDataManager sourceDataManager, ResultDataManager resultDataManager)
    {
        _assetManager = assetManager;
        _sourceDataManager = sourceDataManager;
        _resultDataManager = resultDataManager;

        winterHeatDemands = _sourceDataManager.GetWinterHeatDemands();
        summerHeatDemands = _sourceDataManager.GetSummerHeatDemands();
        
        SortingProductionUnitsByCost();
        SortingProductionUnitsByCO2Emissions();
        UnitProduceDemandedHeat(true, true);
    }
    
    /// <summary>
    /// Runs the optimization process to determine the best heat production schedule based on the given season.
    /// </summary>
    /// <param name="season">The season for which optimization is performed ("summer" or "winter").</param>
    /// <returns>A list of production schedules for each unit.</returns>

        
    
    private List<ProductionUnit> productionUnits = [];
    private List<ProductionUnit> productionUnitsByCost = [];
    private List<ProductionUnit> productionUnitsByCO2Emissions = [];
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];

    private void SortingProductionUnitsByCost()
    {
        productionUnits = _assetManager.GetProductionUnits();
        productionUnitsByCost = productionUnits.Take(3).OrderBy(p => p.ProductionCosts).ToList(); // TODO: Not scalable, should be refactored!

        Console.WriteLine("Production Units sorted by Production Costs:");
        foreach (var productionUnit in productionUnitsByCost)
        {
            Console.WriteLine($"{productionUnit.Name} - {productionUnit.ProductionCosts}");
        }
    }

    private void SortingProductionUnitsByCO2Emissions()
    {
        productionUnits = _assetManager.GetProductionUnits();
        productionUnitsByCO2Emissions = productionUnits.Take(3).OrderBy(p => p.CO2Emissions).ToList(); // TODO: Not scalable, should be refactored!
        Console.WriteLine("Production Units sorted by CO2 Emissions:");
        foreach (var productionUnit in productionUnitsByCO2Emissions)
        {
            Console.WriteLine($"{productionUnit.Name} - {productionUnit.CO2Emissions}");
        }
    }

    private void UnitProduceDemandedHeat(bool isWinter, bool isOptimizedByCost)
    {
        List<HeatDemand> heatDemands = isWinter ? winterHeatDemands : summerHeatDemands;
        List<ProductionUnit> productionUnits = isOptimizedByCost ? productionUnitsByCost : productionUnitsByCO2Emissions;
    
        foreach (var heatDemand in heatDemands)
        {
            double heat = heatDemand.Heat;
            double cost = 0;
            List<ProductionUnit> usedProductionUnits = [];
            int i = 0;
            Console.WriteLine($"Heat demand: {heat} MWh");
            while (heat > 0)
            {
                if (heat > productionUnits[i].MaxHeat)
                {
                    cost += productionUnits[i].ProductionCosts * productionUnits[i].MaxHeat;
                }
                else
                {
                    cost += productionUnits[i].ProductionCosts * heat;
                }
                heat -= productionUnits[i].MaxHeat;
                usedProductionUnits.Add(productionUnits[i]);
                i += 1;
            }
            Console.WriteLine($"Total cost: {Math.Round(cost)} DKK");
            Console.WriteLine($"Used production units: {string.Join(", ", usedProductionUnits.Select(u => u.Name))}");
            Console.WriteLine("-------------------------------------------------");
        }
    }

    private void CalculateProductionCosts()
    {
        // Calculate production costs for each production unit.
        foreach (var productionUnit in productionUnits)
        {
            productionUnit.ProductionCosts = productionUnit.ProductionCosts;
        }
    }
}