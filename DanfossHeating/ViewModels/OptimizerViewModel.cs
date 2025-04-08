using System;
using System.Windows.Input;

namespace DanfossHeating.ViewModels;

public class OptimizerViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Optimizer;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    public OptimizerViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(() => { /* Already on optimizer page */ });
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToSettingsCommand = new Command(NavigateToSettings);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        
        Console.WriteLine($"OptimizerViewModel created for user: {userName}");
    }
    
    private void NavigateToHome()
    {
        if (MainViewModel != null)
        {
            var homeViewModel = new HomePageViewModel(UserName, IsDarkTheme);
            homeViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(homeViewModel);
        }
    }
    
    private void NavigateToCost()
    {
        if (MainViewModel != null)
        {
            var costViewModel = new CostViewModel(UserName, IsDarkTheme);
            costViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(costViewModel);
        }
    }
    
    private void NavigateToCO2Emission()
    {
        if (MainViewModel != null)
        {
            var co2ViewModel = new CO2EmissionViewModel(UserName, IsDarkTheme);
            co2ViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(co2ViewModel);
        }
    }
    
    private void NavigateToSettings()
    {
        if (MainViewModel != null)
        {
            var settingsViewModel = new SettingsViewModel(UserName, IsDarkTheme);
            settingsViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(settingsViewModel);
        }
    }
    
    private void NavigateToAboutUs()
    {
        if (MainViewModel != null)
        {
            var aboutUsViewModel = new AboutUsViewModel(UserName, IsDarkTheme);
            aboutUsViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(aboutUsViewModel);
        }
    }
}
