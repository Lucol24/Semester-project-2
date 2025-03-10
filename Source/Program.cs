namespace DanfossHeating;

class Program
{
    static void Main()
    {
        Console.WriteLine("\nInitializing Asset Manager...\n");
        AssetManager assetManager = new();

        Console.WriteLine("\nInitializing Source Data Manager...\n");
        SourceDataManager sourceDataManager = new();
    }
}