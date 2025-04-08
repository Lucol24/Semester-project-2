using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DanfossHeating.ViewModels;
using System;
using System.ComponentModel;

namespace DanfossHeating.Views;

public class PageBase : UserControl
{
    protected PageViewModelBase? ViewModel { get; private set; }
    
    public PageBase()
    {
        DataContextChanged += PageBase_DataContextChanged;
    }
    
    private void PageBase_DataContextChanged(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        
        ViewModel = DataContext as PageViewModelBase;
        
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateThemeClass();
        }
    }
    
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PageViewModelBase.IsDarkTheme))
        {
            UpdateThemeClass();
        }
    }
    
    protected virtual void UpdateThemeClass()
    {
        try
        {
            if (ViewModel != null)
            {
                Classes.Set("dark", ViewModel.IsDarkTheme);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting theme class: {ex.Message}");
        }
    }
}
