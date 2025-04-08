using Avalonia;
using System;
using System.Linq;

namespace DanfossHeating;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--term"))
        {
            // Run terminal mode
            TermMode.Run();
        }
        else
        {
            // Run Avalonia GUI
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}