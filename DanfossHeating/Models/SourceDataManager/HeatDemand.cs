using System;

namespace DanfossHeating;

/// <summary>
/// Stores winter and summer heat demand records from the CSV
/// </summary>
public class HeatDemand
{
    public DateTime TimeFrom { get; set; } 
    public DateTime TimeTo { get; set; }
    public double Heat { get; set; }
    public double ElectricityPrice { get; set; }
}