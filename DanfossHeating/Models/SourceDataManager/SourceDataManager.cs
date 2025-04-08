using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace DanfossHeating;

public class SourceDataManager
{
    private List<HeatDemand> winterHeatDemands = [];
    private List<HeatDemand> summerHeatDemands = [];
    private readonly string csvPath = "DanfossHeating/Data/heat_demand.csv";

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
        try
        {
            // For test context (tests set working directory to solution root)
            if (File.Exists(csvPath))
            {
                ProcessCsvFile(csvPath);
                return;
            }

            // Data folder accessible from app folder
            string appPath = "Data/heat_demand.csv";
            if (File.Exists(appPath))
            {
                ProcessCsvFile(appPath);
                return;
            }
        }
        catch (Exception ex)
        {
            LogError($"Unexpected Error: {ex.Message}");
        }
    }
    
    private void ProcessCsvFile(string filePath)
    {
        try
        {
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
            LogError($"Error processing CSV file: {ex.Message}");
        }
    }

    public List<HeatDemand> GetWinterHeatDemands() => winterHeatDemands;
    public List<HeatDemand> GetSummerHeatDemands() => summerHeatDemands;

    public List<HeatDemand> GetHeatDemand(DateTime start, DateTime end) 
    {
        return [.. winterHeatDemands.Concat(summerHeatDemands).Where(d => d.TimeFrom >= start && d.TimeTo <= end)];
    }
}