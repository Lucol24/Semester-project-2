using System;
using System.Windows.Input;

namespace DanfossHeating.ViewModels;

public class CostViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Cost;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    public CostViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(() => { /* Already on cost page */ });
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToSettingsCommand = new Command(NavigateToSettings);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        
        Console.WriteLine($"CostViewModel created for user: {userName}");
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
    
    private void NavigateToOptimizer()
    {
        if (MainViewModel != null)
        {
            var viewModel = new OptimizerViewModel(UserName, IsDarkTheme);
            viewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(viewModel);
        }
    }
    
    private void NavigateToCO2Emission()
    {
        if (MainViewModel != null)
        {
            var viewModel = new CO2EmissionViewModel(UserName, IsDarkTheme);
            viewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(viewModel);
        }
    }
    
    private void NavigateToSettings()
    {
        if (MainViewModel != null)
        {
            var viewModel = new SettingsViewModel(UserName, IsDarkTheme);
            viewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(viewModel);
        }
    }
    
    private void NavigateToAboutUs()
    {
        if (MainViewModel != null)
        {
            var viewModel = new AboutUsViewModel(UserName, IsDarkTheme);
            viewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(viewModel);
        }
    }
}
