namespace DanfossHeating;

/// <summary>
/// The Optimizer class is responsible for calculating the optimal heat production schedule.
/// It determines which production units should be used based on cost and efficiency.
/// </summary>
public class Optimizer
{
    // These are dependencies required for the optimization process.
    private readonly AssetManager _assetManager;
    private readonly SourceDataManager _sourceDataManager;
    private readonly ResultDataManager _resultDataManager;
    
    public Optimizer(AssetManager assetManager, SourceDataManager sourceDataManager, ResultDataManager resultDataManager)
    {
        _assetManager = assetManager;
        _sourceDataManager = sourceDataManager;
        _resultDataManager = resultDataManager;

        winterHeatDemands = _sourceDataManager.GetWinterHeatDemands();
        summerHeatDemands = _sourceDataManager.GetSummerHeatDemands();
        
        SortingProductionUnitsByCost();
        SortingProductionUnitsByCO2Emissions();
        SortingProductionUnitsByCost2(true);
        SortingProductionUnitsByCO2Emissions2();
        UnitProduceDemandedHeat(true, true);
        Scenario2(true, true);
    }
    
    /// <summary>
    /// Runs the optimization process to determine the best heat production schedule based on the given season.
    /// </summary>
    /// <param name="season">The season for which optimization is performed ("summer" or "winter").</param>
    /// <returns>A list of production schedules for each unit.</returns>    
    private List<ProductionUnit> productionUnits = [];
    private List<ProductionUnit> productionUnitsByCost = [];
    private List<ProductionUnit> productionUnitsByCO2Emissions = [];
    private List<ProductionUnit> productionUnitsByCost2 = [];
    private List<ProductionUnit> productionUnitsByCO2Emissions2 = [];
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];
    private List<double> electricityPrices = [];

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
            double co2Emissions = 0;
            List<ProductionUnit> usedProductionUnits = [];
            int i = 0;
            Console.WriteLine($"Heat demand: {heat} MWh");
            while (heat > 0) // TODO: Add check for heat demand superior than maxheat that can be provided from all the machines combined
            {
                if (heat > productionUnits[i].MaxHeat)
                {
                    cost += productionUnits[i].ProductionCosts * productionUnits[i].MaxHeat;
                    co2Emissions += productionUnits[i].CO2Emissions * productionUnits[i].MaxHeat;
                }
                else
                {
                    cost += productionUnits[i].ProductionCosts * heat;
                    co2Emissions += productionUnits[i].CO2Emissions * heat;
                }
                heat -= productionUnits[i].MaxHeat;
                usedProductionUnits.Add(productionUnits[i]);
                i += 1;
            }
            Console.WriteLine($"Total cost: {Math.Round(cost)} DKK");
            Console.WriteLine($"Total CO2 emissions: {Math.Round(co2Emissions)} kg");
            Console.WriteLine($"Used production units: {string.Join(", ", usedProductionUnits.Select(u => u.Name))}");
            Console.WriteLine("-------------------------------------------------");
        }
    }

    private void SortingProductionUnitsByCost2(bool isWinter)
    {
        List<HeatDemand> heatDemands = isWinter ? winterHeatDemands : summerHeatDemands;
        foreach(var heatDemand in heatDemands)
        {
            electricityPrices.Add(heatDemand.ElectricityPrice);
        }

        productionUnits = _assetManager.GetProductionUnits();
        productionUnits = productionUnits.Where(p => p.Name != "GB2").ToList(); // TODO: Not scalable, should be refactored!

        Console.WriteLine("Production Units sorted by Production Costs:");
        foreach (var productionUnit in productionUnitsByCost2)
        {
            Console.WriteLine($"{productionUnit.Name} - {productionUnit.ProductionCosts}");
        }
    }

    private void SortingProductionUnitsByCO2Emissions2()
    {
        productionUnits = _assetManager.GetProductionUnits();
        productionUnitsByCO2Emissions2 = productionUnits.Where(p => p.Name != "GB2").OrderBy(p => p.CO2Emissions).ToList(); // TODO: Not scalable, should be refactored!
        Console.WriteLine("Production Units sorted by CO2 Emissions:");
        foreach (var productionUnit in productionUnitsByCO2Emissions2)
        {
            Console.WriteLine($"{productionUnit.Name} - {productionUnit.CO2Emissions}");
        }
    }

    private void Scenario2(bool isWinter, bool isOptimizedByCost)
    {
        List<HeatDemand> heatDemands = isWinter ? winterHeatDemands : summerHeatDemands;
        List<ProductionUnit> productionUnits = isOptimizedByCost ? productionUnitsByCost2 : productionUnitsByCO2Emissions2;

        foreach (var heatDemand in heatDemands)
        {
            double heat = heatDemand.Heat;
            double cost = 0;
            double co2Emissions = 0;
            List<ProductionUnit> usedProductionUnits = [];
            int i = 0;
            Console.WriteLine($"Heat demand: {heat} MWh");
            while (heat > 0)
            {
                if (heat > productionUnits[i].MaxHeat)
                {
                    cost += productionUnits[i].ProductionCosts * productionUnits[i].MaxHeat;
                    co2Emissions += productionUnits[i].CO2Emissions * productionUnits[i].MaxHeat;
                }
                else
                {
                    cost += productionUnits[i].ProductionCosts * heat;
                    co2Emissions += productionUnits[i].CO2Emissions * heat;
                }
                heat -= productionUnits[i].MaxHeat;
                usedProductionUnits.Add(productionUnits[i]);
                i += 1;
            }
            Console.WriteLine($"Total cost: {Math.Round(cost)} DKK");
            Console.WriteLine($"Total CO2 emissions: {Math.Round(co2Emissions)} kg");
            Console.WriteLine($"Used production units: {string.Join(", ", usedProductionUnits.Select(u => u.Name))}");
            Console.WriteLine("-------------------------------------------------");
        }
    }
}