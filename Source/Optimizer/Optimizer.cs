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

    public enum OptimizationCriteria
    {
        Cost,
        CO2Emissions
    }

    public List<ProductionSchedule> OptimizeHeatProduction(string season, OptimizationCriteria criteria = OptimizationCriteria.Cost)
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

        var productionUnits = GetScenario1Units(); // Implement bool for scenario 2 here

        foreach (var demand in heatDemands)
        {
            double remainingHeat = demand.Heat;

            var sortedUnits = criteria == OptimizationCriteria.CO2Emissions 
                ? SortUnitsByEcologicalImpact(productionUnits)
                : SortUnitsByCost(productionUnits);

            foreach (var unit in sortedUnits)
            {
                if (remainingHeat <= 0) break;

                double allocatableHeat = Math.Min(remainingHeat, unit.MaxHeat ?? 0);
                remainingHeat -= allocatableHeat;

                var resultEntry = CreateResultEntry(unit, demand, allocatableHeat);
                results.Add(resultEntry);
                AddToSchedule(schedules, resultEntry);
            }
        }

        _resultDataManager.SaveResults(results);
        return schedules;
    }

    private List<ProductionUnit> SortUnitsByCost(List<ProductionUnit> units)
    {
        return units
            .OrderBy(unit => unit.ProductionCosts ?? double.MaxValue)
            .ToList();
    }

    private List<ProductionUnit> SortUnitsByEcologicalImpact(List<ProductionUnit> units)
    {
        return units
            .OrderBy(unit => unit.CO2Emissions ?? double.MaxValue)
            .ToList();
    }

    private List<ProductionUnit> GetScenario1Units()
    {
        var allUnits = _assetManager.GetProductionUnits();
        return allUnits.Where(u => u.Name == "GB1" || u.Name == "GB2" || u.Name == "OB1").ToList();
    }

    private ResultEntry CreateResultEntry(ProductionUnit unit, HeatDemand demand, double heatProduced)
    {
        double productionCost = (unit.ProductionCosts ?? 0) * heatProduced;
        double fuelConsumption = (unit.FuelConsumption ?? 0) * heatProduced;
        double co2Emissions = (unit.CO2Emissions ?? 0) * heatProduced;

        return new ResultEntry(
            unit.Name ?? "Unknown",
            demand.TimeFrom,
            Math.Round(heatProduced, 2),
            0, // Here goes the electricity produced, if applicable
            Math.Round(productionCost, 2),
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