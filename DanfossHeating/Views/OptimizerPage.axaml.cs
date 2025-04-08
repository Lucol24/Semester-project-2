using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;
using System.ComponentModel;

namespace DanfossHeating.Views;

public partial class OptimizerPage : PageBase
{
    public OptimizerPage()
    {
        InitializeComponent();
        Console.WriteLine("OptimizerPage constructed");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
