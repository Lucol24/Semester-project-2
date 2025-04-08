using System;

namespace DanfossHeating;

public static class TermMode
{
    public static void Run()
    {
        // Application header
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n╔═══════════════════════════════════════════════╗");
        Console.WriteLine("║       DANFOSS HEATING OPTIMIZATION SYSTEM     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════╝");
        Console.ResetColor();

        // Initialize components
        DisplaySectionHeader("Initializing System Components");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("► Initializing Asset Manager");
        Console.ResetColor();
        AssetManager assetManager = new();
        Console.WriteLine(" ✓");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("► Initializing Source Data Manager");
        Console.ResetColor();
        SourceDataManager sourceDataManager = new();
        Console.WriteLine(" ✓");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("► Initializing Result Data Manager");
        Console.ResetColor();
        ResultDataManager resultDataManager = new();
        Console.WriteLine(" ✓");

        // User input section
        DisplaySectionHeader("Configuration Settings");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Season [winter / summer]: ");
        Console.ResetColor();
        string season = Console.ReadLine()?.Trim().ToLower() == "summer" ? "summer" : "winter";
        Console.WriteLine($"Selected: {season}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Scenario [1 or 2]: ");
        Console.ResetColor();
        bool isScenario2 = Console.ReadLine()?.Trim() == "2";
        Console.WriteLine($"Selected: Scenario {(isScenario2 ? "2" : "1")}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Optimization Criteria [1 = Cost / 2 = CO2 Emissions]: ");
        Console.ResetColor();
        string criteriaInput = Console.ReadLine() ?? string.Empty;
        var criteria = OptimizationCriteria.Cost;
        
        if (criteriaInput.Trim() == "2")
        {
            criteria = OptimizationCriteria.CO2Emissions;
        }
        Console.WriteLine($"Selected: {criteria} optimization");

        // Process section
        DisplaySectionHeader("Optimization Process");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Running Heat Production Optimization with {criteria} criteria...");
        Console.ResetColor();
        
        Optimizer optimizer = new(assetManager, sourceDataManager, resultDataManager);
        optimizer.OptimizeHeatProduction(season, criteria, isScenario2);

        // Results section
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Optimization complete!");
        Console.WriteLine("✓ Results saved to 'Data/result_data.csv'");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n╔═══════════════════════════════════════════════╗");
        Console.WriteLine("║                PROCESS COMPLETED              ║");
        Console.WriteLine("╚═══════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    private static void DisplaySectionHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"■ {title} ".PadRight(50, '─'));
        Console.ResetColor();
        Console.WriteLine();
    }
}