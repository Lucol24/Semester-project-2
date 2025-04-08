using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;

namespace DanfossHeating.Views;

public partial class CO2EmissionPage : UserControl
{
    private CO2EmissionViewModel? _viewModel;
    
    public CO2EmissionPage()
    {
        InitializeComponent();
        Console.WriteLine("CO2EmissionPage constructed");
        
        Loaded += Page_Loaded;
        DataContextChanged += Page_DataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void Page_Loaded(object? sender, EventArgs e)
    {
        Console.WriteLine("CO2EmissionPage loaded and visible");
        UpdateThemeClass();
    }
    
    private void Page_DataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        
        _viewModel = DataContext as CO2EmissionViewModel;
        
        if (_viewModel != null)
        {
            Console.WriteLine($"CO2EmissionPage received DataContext with userName: {_viewModel.UserName}");
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateThemeClass();
        }
        else
        {
            Console.WriteLine("WARNING: CO2EmissionPage DataContext is not CO2EmissionViewModel");
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CO2EmissionViewModel.IsDarkTheme))
        {
            UpdateThemeClass();
        }
    }
    
    private void UpdateThemeClass()
    {
        try
        {
            if (_viewModel != null)
            {
                Classes.Set("dark", _viewModel.IsDarkTheme);
                Console.WriteLine($"Updated CO2EmissionPage theme class: {(_viewModel.IsDarkTheme ? "dark" : "light")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting theme class: {ex.Message}");
        }
    }
}
