using System;
using System.Windows.Input;

namespace DanfossHeating.ViewModels;

public class CO2EmissionViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.CO2Emission;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    public CO2EmissionViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(() => { /* Already on CO2 page */ });
        NavigateToSettingsCommand = new Command(NavigateToSettings);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        
        Console.WriteLine($"CO2EmissionViewModel created for user: {userName}");
    }
    
    private void NavigateToHome()
    {
        if (MainViewModel != null)
        {
            var viewModel = new HomePageViewModel(UserName, IsDarkTheme);
            viewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(viewModel);
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
    
    private void NavigateToCost()
    {
        if (MainViewModel != null)
        {
            var viewModel = new CostViewModel(UserName, IsDarkTheme);
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
