using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DanfossHeating;

public class AssetManager
{
    private List<ProductionUnit> productionUnits = [];
    private readonly string JsonPath = "DanfossHeating/Data/production_units.json";
    
    public AssetManager()
    {
        LoadProductionUnits();
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void LoadProductionUnits()
    {
        try
        {
            // For test context (tests set working directory to solution root)
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? [];
                return;
            }

            // Data folder accessible from app folder
            string appPath = "Data/production_units.json";
            if (File.Exists(appPath))
            {
                string json = File.ReadAllText(appPath);
                productionUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? [];
                return;
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
    
    public void SaveProductionUnit(ProductionUnit unit)
    {
        try
        {
            // Find and update the unit in our list
            int index = productionUnits.FindIndex(u => u.Name == unit.Name);
            if (index >= 0)
            {
                productionUnits[index] = unit;
            }
            
            // Save the updated list to the JSON file
            string json = JsonSerializer.Serialize(productionUnits, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            // Try to save to the default path
            string filePath = JsonPath;
            if (!File.Exists(filePath))
            {
                // Try the app data path
                filePath = "Data/production_units.json";
                
                // Ensure directory exists
                string? directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            
            File.WriteAllText(filePath, json);
            Console.WriteLine($"Successfully saved changes to {filePath}");
        }
        catch (Exception ex)
        {
            LogError($"Error saving production unit: {ex.Message}");
            throw; // Re-throw so the ViewModel can handle it
        }
    }
}