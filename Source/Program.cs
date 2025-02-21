namespace DanfossHeating;

class Program
{
    static void Main()
    {
        Console.WriteLine("\nInitializing Asset Manager...\n");
        
        AssetManager assetManager = new();
        assetManager.DisplayData();
    }
}