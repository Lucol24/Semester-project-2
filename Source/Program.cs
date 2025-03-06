namespace DanfossHeating;

class Program
{
    static void Main()
    {
        Console.WriteLine("\nInitializing Asset Manager...\n");
        AssetManager assetManager = new();
        assetManager.DisplayData();

        Console.WriteLine("\nInitializing Source Data Manager...\n");
        SourceDataManager sourceDataManager = new();

        Console.WriteLine("\n---❄️  Winter Heat Demand ❄️  ---\n");
        foreach (var demand in sourceDataManager.GetWinterHeatDemands())
        {
            Console.WriteLine($"From {demand.TimeFrom} to {demand.TimeTo}, Heat: {demand.Heat} MWh, Electricity Price: {demand.ElectricityPrice} DKK/MWh");
        }

        Console.WriteLine("\n\n--- ☀️  Summer Heat Demand ☀️  ---\n");
        foreach (var demand in sourceDataManager.GetSummerHeatDemands())
        {
            Console.WriteLine($"From {demand.TimeFrom} to {demand.TimeTo}, Heat: {demand.Heat} MWh, Electricity Price: {demand.ElectricityPrice} DKK/MWh");
        }
    }
}