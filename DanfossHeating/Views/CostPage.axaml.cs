using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace DanfossHeating.Views;

public partial class CostPage : PageBase
{
    public CostPage(string userName, bool isDarkTheme)
    {
        InitializeComponent();
        DataContext = new CostViewModel(userName, isDarkTheme);
        Console.WriteLine("CostPage constructed");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
