using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;

namespace DanfossHeating.ViewModels;

public abstract class PageViewModelBase : ViewModelBase
{
    protected string _userName = string.Empty;
    protected string _userInitial = string.Empty;
    protected bool _isDarkTheme;
    
    public string UserName
    {
        get => _userName;
        set
        {
            if (SetProperty(ref _userName, value))
            {
                UserInitial = !string.IsNullOrEmpty(value) ? value[0].ToString().ToUpper() : "?";
            }
        }
    }

    public string UserInitial
    {
        get => _userInitial;
        protected set => SetProperty(ref _userInitial, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                UpdateTheme(value);
            }
        }
    }
    
    public ICommand ToggleThemeCommand { get; }
    
    protected MainWindowViewModel? MainViewModel { get; private set; }
    
    public PageViewModelBase(string userName, bool isDarkTheme)
    {
        _userName = userName;
        _userInitial = !string.IsNullOrEmpty(userName) ? userName[0].ToString().ToUpper() : "?";
        _isDarkTheme = isDarkTheme;
        ToggleThemeCommand = new Command(ToggleTheme);
    }
    
    public virtual void SetMainViewModel(MainWindowViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
    }
    
    public virtual void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }
    
    protected virtual void UpdateTheme(bool isDark)
    {
        try
        {
            Application.Current!.RequestedThemeVariant = 
                isDark ? ThemeVariant.Dark : ThemeVariant.Light;
            Console.WriteLine($"Theme changed to {(isDark ? "Dark" : "Light")} in {GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating theme: {ex.Message}");
        }
    }
}
