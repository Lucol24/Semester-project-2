using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;

namespace DanfossHeating.Views;
public partial class HomePage : UserControl
{
    private HomePageViewModel? _viewModel;
    
    public HomePage()
    {
        InitializeComponent();
        Console.WriteLine("HomePage constructed");
        
        Loaded += HomePage_Loaded;
        DataContextChanged += HomePage_DataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void HomePage_Loaded(object? sender, EventArgs e)
    {
        Console.WriteLine("HomePage loaded and visible");
        UpdateThemeClass();
    }
    
    private void HomePage_DataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        
        _viewModel = DataContext as HomePageViewModel;
        
        if (_viewModel != null)
        {
            Console.WriteLine($"HomePage received DataContext with userName: {_viewModel.UserName}");
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateThemeClass();
        }
        else
        {
            Console.WriteLine("WARNING: HomePage DataContext is not HomePageViewModel");
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomePageViewModel.IsDarkTheme))
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
                Console.WriteLine($"Updated HomePage theme class: {(_viewModel.IsDarkTheme ? "dark" : "light")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting theme class: {ex.Message}");
        }
    }
}