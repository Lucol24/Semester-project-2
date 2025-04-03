namespace DanfossHeating;

public class Optimizer
{
    private readonly AssetManager _assetManager;
    private readonly SourceDataManager _sourceDataManager;
    private readonly ResultDataManager _resultDataManager;

    public Optimizer(AssetManager assetManager, SourceDataManager sourceDataManager, ResultDataManager resultDataManager)
    {
        _assetManager = assetManager;
        _sourceDataManager = sourceDataManager;
        _resultDataManager = resultDataManager;
    }

    public List<ProductionSchedule> OptimizeHeatProduction(string season, OptimizationCriteria criteria = OptimizationCriteria.Cost, bool isScenario2=false)
    {
        var schedules = new List<ProductionSchedule>();
        var results = new List<ResultEntry>();

        var heatDemands = season.ToLower() == "summer" // To be changed when UI is implemented
            ? _sourceDataManager.GetSummerHeatDemands() 
            : _sourceDataManager.GetWinterHeatDemands();

        if (heatDemands.Count == 0)
        {
            Console.WriteLine($"No heat demand data found for {season}. Optimization cannot proceed.");
            return schedules;
        }

        var productionUnits = GetScenarioUnits(isScenario2); 

        foreach (var demand in heatDemands)
        {
            double remainingHeat = demand.Heat;

            var sortedUnits = criteria == OptimizationCriteria.CO2Emissions 
                ? SortUnitsByEcologicalImpact(productionUnits)
                : SortUnitsByCost(productionUnits, isScenario2, demand);

            foreach (var unit in sortedUnits)
            {
                if (remainingHeat <= 0) break;

                double allocatableHeat = Math.Min(remainingHeat, unit.MaxHeat ?? 0);
                remainingHeat -= allocatableHeat;

                var resultEntry = CreateResultEntry(unit, demand, allocatableHeat, isScenario2);
                results.Add(resultEntry);
                AddToSchedule(schedules, resultEntry);
            }
        }

        _resultDataManager.SaveResults(results);
        return schedules;
    }

    private List<dynamic> SortUnitsByCost(List<ProductionUnit> units, bool isScenario2, HeatDemand demand)
    {
        return units
            .Select(unit => new 
            {
                Unit = unit,
                NetCost = CalculateNetCost(unit, demand, isScenario2, demand.Heat)
            })
            .OrderBy(x => x.NetCost)
            .ToList<dynamic>();
    }

    private List<dynamic> SortUnitsByEcologicalImpact(List<ProductionUnit> units)
    {
        return units
            .Select(unit => new
            {
                Unit = unit,
                CO2Impact = unit.CO2Emissions ?? 0
            })
            .OrderBy(x => x.CO2Impact)
            .ToList<dynamic>();
    }

    private List<ProductionUnit> GetScenarioUnits(bool isScenario2)
    {

        var allUnits = _assetManager.GetProductionUnits();
        if(isScenario2)
        {
            // Scenario 2: Filter out GB2 and include only GB1, GB2, and OB1
            return allUnits.Where(u => u.Name == "GB1" || u.Name == "OB1" || u.Name == "GM1" || u.Name == "HP1").ToList();
        }
        else
        {
            // Scenario 1: Filter out GM1 and HP1, include only GB1, GB2, and OB1
            return allUnits.Where(u => u.Name == "GB1" || u.Name == "GB2" || u.Name == "OB1").ToList();
        }
    }

    private double CalculateElectricityImpact(ProductionUnit unit, double heatAmount, HeatDemand demand, bool isScenario2)
    {
        if(!isScenario2 || !unit.MaxElectricity.HasValue) return 0;

        double electricityImpactPerMWh = (unit.MaxHeat ?? 1) == 0 ? 0 : unit.MaxElectricity.Value / (unit.MaxHeat ?? 1);
        double totalElectricityImpact = electricityImpactPerMWh * heatAmount * demand.ElectricityPrice;
        return unit.MaxElectricity.Value < 0 ? 
            Math.Abs(totalElectricityImpact) // HP1
            : -totalElectricityImpact; // GM1
    }

    private double CalculateNetCost(ProductionUnit unit, HeatDemand demand, bool isScenario2, double heatAmount)
    {
        double baseCostPerMWh = unit.ProductionCosts ?? 0;
        double netCost = baseCostPerMWh * heatAmount;
        netCost += CalculateElectricityImpact(unit, heatAmount, demand, isScenario2);
        return netCost;
    }

    private ResultEntry CreateResultEntry(ProductionUnit unit, HeatDemand demand, double heatProduced, bool isScenario2)
    {
        double totalProductionCost = CalculateNetCost(unit, demand, isScenario2, heatProduced);
        double fuelConsumption = (unit.FuelConsumption ?? 0) * heatProduced;
        double co2Emissions = (unit.CO2Emissions ?? 0) * heatProduced;

        return new ResultEntry(
            unit.Name ?? "Unknown",
            demand.TimeFrom,
            Math.Round(heatProduced, 2),
            Math.Round(isScenario2 ? (unit.MaxElectricity ?? 0) : 0, 2), // Add electricity produced if applicable
            Math.Round(totalProductionCost, 2),
            Math.Round(fuelConsumption, 2),
            Math.Round(co2Emissions, 2)
        );
    }

    private void AddToSchedule(List<ProductionSchedule> schedules, ResultEntry resultEntry)
    {
        var schedule = schedules.FirstOrDefault(s => s.UnitName == resultEntry.UnitName);
        if (schedule == null)
        {
            schedule = new ProductionSchedule(resultEntry.UnitName);
            schedules.Add(schedule);
        }
        schedule.AddEntry(resultEntry);
    }
}