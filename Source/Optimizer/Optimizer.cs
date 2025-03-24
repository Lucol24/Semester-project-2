using Microsoft.VisualBasic;

namespace DanfossHeating;

public class Optimizer
{
    SourceDataManager sourceDataManager = new SourceDataManager();
    AssetManager assetManager = new AssetManager();
    private List<ProductionUnit> productionUnitsByCost = [];
    private List<ProductionUnit> productionUnitsByCO2Emissions = [];
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];

    public Optimizer()
    {
        winterHeatDemands = sourceDataManager.GetWinterHeatDemands();
        summerHeatDemands = sourceDataManager.GetSummerHeatDemands();
        SortingProductionUnitsSC1ByCost();
        SortingProductionUnitsSC1ByCO2Emissions();
        ProduceDemandedHeat(true, true);
    }

    private void SortingProductionUnitsSC1ByCost() // makes more money for the company (scenario 1)
    {
        productionUnitsByCost = assetManager.GetProductionUnits().Take(3).OrderBy(p => p.ProductionCosts).ToList(); // take everything and remove HP and GM
    }

    private void SortingProductionUnitsSC1ByCO2Emissions() // better for the planet (scenario 1)
    {
        productionUnitsByCO2Emissions = assetManager.GetProductionUnits().Take(3).OrderBy(p => p.CO2Emissions).ToList();
    }

    private void ProduceDemandedHeat(bool isWinter, bool isOptimezedByCost)
    {
        List<HeatDemand> heatDemands = isWinter ? winterHeatDemands : summerHeatDemands;
        List<ProductionUnit> productionUnits = isOptimezedByCost ? productionUnitsByCost : productionUnitsByCO2Emissions;
    
        foreach (var heatDemand in heatDemands)
        {
            double heat = heatDemand.Heat;

            if (8 < heat)
            {
                heat -= productionUnits[0].MaxHeat + productionUnits[1].MaxHeat  + productionUnits[2].MaxHeat;
            }
            else if (7 < heat)
            {
                heat -= productionUnits[0].MaxHeat + productionUnits[2].MaxHeat;
            }
            else if (heat <= 4)
            {
                heat -= productionUnits[0].MaxHeat;
            }
            else
            {
                heat -= productionUnits[0].MaxHeat + productionUnits[1].MaxHeat;
            }
        }
    }
}