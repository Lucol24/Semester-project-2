using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace DanfossHeating;

public class ResultDataManager
{
    private readonly string _filePath;

    public ResultDataManager(string filePath = "Data/result_data.csv")
    {
        _filePath = filePath;
    }

    public void SaveResults(List<ResultEntry> results)
    {
        using var writer = new StreamWriter(_filePath, append: false); // Append to false (overwrite)
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<ResultEntry>(); // Write headers for new data
        csv.NextRecord();
        csv.WriteRecords(results);
    }

    public List<ResultEntry> LoadResults()
    {
        var results = new List<ResultEntry>();

        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"File not found: {_filePath}");
            return results;
        }

        try
        {
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null // Ignore missing fields
            });

            csv.Read(); 
            csv.ReadHeader(); 

            while (csv.Read())
            {
                try
                {
                    results.Add(new ResultEntry(
                        csv.GetField<string>("UnitName") ?? string.Empty,
                        csv.GetField<DateTime>("Timestamp"),
                        csv.GetField<double>("HeatProduced"),
                        csv.GetField<double>("ElectricityProduced"),
                        csv.GetField<double>("ProductionCost"),
                        csv.GetField<double>("FuelConsumption"),
                        csv.GetField<double>("CO2Emissions")
                    ));
                }
                catch (CsvHelperException che)
                {
                    Console.WriteLine($"Error reading record: {che.Message}");
                }
            }
            return results;
        }
        catch (HeaderValidationException hve)
        {
            Console.WriteLine($"Error in CSV column names: {hve.Message}");
        }
        catch (CsvHelperException che)
        {
            Console.WriteLine($"Error reading CSV: {che.Message}");
        }
        catch (IOException ioex)
        {
            Console.WriteLine($"I/O Error: {ioex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unknown error loading results: {ex.Message}");
        }
        return results;
    }
}