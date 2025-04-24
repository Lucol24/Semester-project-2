using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;
using System.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace DanfossHeating.Views;

public partial class OptimizerPage : PageBase
{
    private OptimizerViewModel? _viewModel;
    private const double SMALL_SCREEN_WIDTH_THRESHOLD = 800;
    
    public OptimizerPage()
    {
        InitializeComponent();
        Console.WriteLine("OptimizerPage constructed");
        
        Loaded += Page_Loaded;
        DataContextChanged += Page_DataContextChanged;
        SizeChanged += Page_SizeChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void Page_Loaded(object? sender, EventArgs e)
    {
        Console.WriteLine("OptimizerPage loaded and visible");
        UpdateThemeClass();
    }
    
    private void Page_DataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        
        _viewModel = DataContext as OptimizerViewModel;
        
        if (_viewModel != null)
        {
            Console.WriteLine($"OptimizerPage received DataContext with userName: {_viewModel.UserName}");
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateThemeClass();
        }
        else
        {
            Console.WriteLine("WARNING: OptimizerPage DataContext is not OptimizerViewModel");
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OptimizerViewModel.IsDarkTheme))
        {
            UpdateThemeClass();
        }
    }
    
    protected override void UpdateThemeClass()
    {
        try
        {
            if (_viewModel != null)
            {
                Classes.Set("dark", _viewModel.IsDarkTheme);
                Console.WriteLine($"Updated OptimizerPage theme class: {(_viewModel.IsDarkTheme ? "dark" : "light")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting theme class: {ex.Message}");
        }
    }
    
    private void Page_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            // Adjust chart size based on container size
            var chart = this.FindControl<CartesianChart>("chart");
            if (chart != null)
            {
                // Calculate appropriate padding based on screen size
                double horizontalPadding = e.NewSize.Width < SMALL_SCREEN_WIDTH_THRESHOLD ? 20 : 100;
                
                // Update chart dimensions
                _viewModel.ChartWidth = Math.Max(300, e.NewSize.Width - horizontalPadding);
                _viewModel.ChartHeight = Math.Max(200, e.NewSize.Height * 0.6);
                
                // Set responsive button width
                _viewModel.ButtonMaxWidth = Math.Min(775, e.NewSize.Width - 40);
                
                // Update control panel layout based on screen size
                _viewModel.IsCompactMode = e.NewSize.Width < SMALL_SCREEN_WIDTH_THRESHOLD;
                
                Console.WriteLine($"Adjusted UI for width: {e.NewSize.Width}, height: {e.NewSize.Height}");
            }
        }
    }
}
