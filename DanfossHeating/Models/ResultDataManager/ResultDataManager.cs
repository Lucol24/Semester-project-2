using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DanfossHeating;

public class ResultDataManager
{
    private readonly string _filePath = "Data/result_data.csv";

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
        if (!File.Exists(_filePath)) return new List<ResultEntry>();

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return [.. csv.GetRecords<ResultEntry>()];
    }
}