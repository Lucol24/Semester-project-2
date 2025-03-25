namespace DanfossHeating;

public class ResultEntry
{
    public DateTime Timestamp { get; set; }
    public string UnitName { get; set; }
    public double HeatProduced { get; set; }
    public double ElectricityProduced { get; set; }
    public double ProductionCost { get; set; }
    public double PrimaryEnergyConsumption { get; set; }
    public double CO2Emissions { get; set; }

    public ResultEntry(string unitName, DateTime timestamp, double heatProduced, 
                        double electricityProduced, double productionCost, 
                        double primaryEnergyConsumption, double co2Emissions)
    {
        UnitName = unitName;
        Timestamp = timestamp;
        HeatProduced = heatProduced;
        ElectricityProduced = electricityProduced;
        ProductionCost = productionCost;
        PrimaryEnergyConsumption = primaryEnergyConsumption;
        CO2Emissions = co2Emissions;
    }
}