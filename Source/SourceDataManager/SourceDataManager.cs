using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DanfossHeating;

public class SourceDataManager
{
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];

    public SourceDataManager()
    {
        LoadHeatDemand();
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void LoadHeatDemand()
    {
        string filePath = "Data/heat_demand.csv";
        
        try
        {
            if (!File.Exists(filePath))
            {
                LogError("Error: Heat demand file not found.");
                return;
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false });

            for (int i = 0; i < 3; i++) // Skip the first 3 lines of the CSV file (Headers)
            {
                if (reader.ReadLine() == null)
                {
                    throw new InvalidDataException("Error: Unexpected end of file while skipping header lines.");
                }
            }

            while (csv.Read()) 
            {
                try
                {
                    winterHeatDemands.Add(new HeatDemand
                    {
                        TimeFrom = csv.GetField<DateTime>(0),
                        TimeTo = csv.GetField<DateTime>(1),
                        Heat = csv.GetField<double>(2),
                        ElectricityPrice = csv.GetField<double>(3),
                    });

                    summerHeatDemands.Add(new HeatDemand
                    {
                        TimeFrom = csv.GetField<DateTime>(5),
                        TimeTo = csv.GetField<DateTime>(6),
                        Heat = csv.GetField<double>(7),
                        ElectricityPrice = csv.GetField<double>(8),
                    });
                }
                catch (CsvHelperException ex)
                {
                    LogError($"CSV Parsing Error at row {csv.Context.Parser?.Row ?? -1}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"Error loading heat demand data: {ex.Message}");
        }
    }

    public List<HeatDemand> GetWinterHeatDemands() => winterHeatDemands;
    public List<HeatDemand> GetSummerHeatDemands() => summerHeatDemands;

    public List<HeatDemand> GetHeatDemand(DateTime start, DateTime end)
    {
        return [.. winterHeatDemands.Concat(summerHeatDemands).Where(d => d.TimeFrom >= start && d.TimeTo <= end)];
    }
}