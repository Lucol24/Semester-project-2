using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;
using System.ComponentModel;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace DanfossHeating.Views;

public partial class OptimizerPage : PageBase
{
    private OptimizerViewModel? _viewModel;
    private const double SMALL_SCREEN_WIDTH_THRESHOLD = 800;
    private CartesianChart? _chart;
    
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
        _chart = this.FindControl<CartesianChart>("chart");
        if (_chart != null)
        {
            Console.WriteLine("Found chart control during initialization");
        }
    }
    
    private void Page_Loaded(object? sender, EventArgs e)
    {
        Console.WriteLine("OptimizerPage loaded and visible");
        UpdateThemeClass();
        
        // Try to find chart again if not found during initialization
        if (_chart == null)
        {
            _chart = this.FindControl<CartesianChart>("chart");
            if (_chart != null)
            {
                Console.WriteLine("Found chart control after page load");
            }
        }
        
        // Pass the chart reference to the ViewModel if available
        if (_viewModel != null && _chart != null)
        {
            _viewModel.SetChartControl(_chart);
        }
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
            
            // Pass the chart reference to the new ViewModel if available
            if (_chart != null)
            {
                _viewModel.SetChartControl(_chart);
            }
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
            if (_chart != null)
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
