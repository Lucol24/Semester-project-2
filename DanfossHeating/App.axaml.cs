using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using DanfossHeating.Views;
using System;

namespace DanfossHeating;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var mainViewModel = new MainWindowViewModel();
            
            Console.WriteLine("Application startup: Setting MainWindow DataContext");
            mainWindow.DataContext = mainViewModel;
            
            desktop.MainWindow = mainWindow;
            
            // Subscribe to unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Console.WriteLine($"UNHANDLED EXCEPTION: {args.ExceptionObject}");
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}