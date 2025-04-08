using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;

namespace DanfossHeating.Views;

public partial class CostPage : PageBase
{
    public CostPage()
    {
        InitializeComponent();
        Console.WriteLine("CostPage constructed");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
