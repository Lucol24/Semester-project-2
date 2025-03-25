using Microsoft.VisualBasic;
 
 namespace DanfossHeating;
 
 public class Optimizer
 {
     SourceDataManager sourceDataManager = new SourceDataManager();
     AssetManager assetManager = new AssetManager();
     private List<ProductionUnit> productionUnits = [];
     private List<ProductionUnit> productionUnitsByCost = [];
     private List<ProductionUnit> productionUnitsByCO2Emissions = [];
     private List<HeatDemand> winterHeatDemands = [];
     private List<HeatDemand> summerHeatDemands = [];
 
     public Optimizer()
     {
         winterHeatDemands = sourceDataManager.GetWinterHeatDemands();
         summerHeatDemands = sourceDataManager.GetSummerHeatDemands();
         SortingProductionUnitsByCost();
         SortingProductionUnitsByCO2Emissions();
         ProduceDemandedHeat(true, true);
     }
 
     private void SortingProductionUnitsByCost()
     {
         productionUnits = assetManager.GetProductionUnits();
         productionUnitsByCost = productionUnits.Take(3).OrderBy(p => p.ProductionCosts).ToList();
 
         Console.WriteLine("Production Units sorted by Production Costs:");
         foreach (var productionUnit in productionUnitsByCost)
         {
             Console.WriteLine($"{productionUnit.Name} - {productionUnit.ProductionCosts}");
         }
     }
 
     private void SortingProductionUnitsByCO2Emissions()
     {
         productionUnits = assetManager.GetProductionUnits();
         productionUnitsByCO2Emissions = productionUnits.Take(3).OrderBy(p => p.CO2Emissions).ToList();
         Console.WriteLine("Production Units sorted by CO2 Emissions:");
         foreach (var productionUnit in productionUnitsByCO2Emissions)
         {
             Console.WriteLine($"{productionUnit.Name} - {productionUnit.CO2Emissions}");
         }
     }
 
     private void ProduceDemandedHeat(bool isWinter, bool isOptimezedByCost)
     {
         List<HeatDemand> heatDemands = isWinter ? winterHeatDemands : summerHeatDemands;
         List<ProductionUnit> productionUnits = isOptimezedByCost ? productionUnitsByCost : productionUnitsByCO2Emissions;
     
         foreach (var heatDemand in heatDemands)
         {
             double heat = heatDemand.Heat;
             int i = 0;
             while (heat > 0)
             {
                 Console.WriteLine($"Heat demand: {heat}");
                 Console.WriteLine($"Production Unit used: {productionUnits[i].Name}");
                 heat -= productionUnits[i].MaxHeat;
                 i += 1;
             }
         }
     }
 }