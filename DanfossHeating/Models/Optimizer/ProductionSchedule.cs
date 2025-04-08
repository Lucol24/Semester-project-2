using System.Collections.Generic;

namespace DanfossHeating;

/// <summary>
/// Represents a production schedule for a specific production unit.
/// Stores all result entries related to this unit's heat production.
/// </summary>

public class ProductionSchedule(string unitName)
{
    public string UnitName { get; } = unitName;
    public List<ResultEntry> Schedule { get; } = [];

    public void AddEntry(ResultEntry entry)
    {
        Schedule.Add(entry);
    }
}