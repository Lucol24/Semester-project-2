using System.Text.Json;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = [];
    public AssetManager()
    {
        LoadProductionUnits(); // Load production unit data from JSON file
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void LoadProductionUnits()
    {
        string filePath = "Data/production_units.json";
        
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? [];
            }
            else
            {
                LogError("Error: Production units file not found.");
            }
        }
        catch (IOException ex)
        {
            LogError($"I/O Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            LogError($"JSON Parsing Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError($"Unexpected Error: {ex.Message}");
        }
    }

    public List<ProductionUnit> GetProductionUnits() => productionUnits;
}