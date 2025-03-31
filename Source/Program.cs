namespace DanfossHeating;

class Program
{
    static void Main()
    {
        Console.WriteLine("\nInitializing Asset Manager...\n");
        AssetManager assetManager = new();

        Console.WriteLine("\nInitializing Source Data Manager...\n");
        SourceDataManager sourceDataManager = new();

        Console.WriteLine("\nInitializing Optimizer...\n");
        ResultDataManager resultDataManager = new();
        
        Console.WriteLine("Summer or Winter?");
        string? season = Console.ReadLine()?.Trim().ToLower() == "summer" ? "summer" : "winter";

        /* Console.WriteLine("Scenario 1 or Scenario 2?");
        bool IsScenario2 = Console.ReadLine()?.Trim().ToLower() == "2"; */

        Console.WriteLine("Cost or CO2?");
        string? criteriaInput = Console.ReadLine();

        var criteria = Optimizer.OptimizationCriteria.Cost;
        if (criteriaInput == "2") // Cost is 1 so CO2 is 2
        {
            criteria = Optimizer.OptimizationCriteria.CO2Emissions;
        }

        Optimizer optimizer = new(assetManager, sourceDataManager, resultDataManager);
        optimizer.OptimizeHeatProduction(season, criteria);
    

        
    }
}