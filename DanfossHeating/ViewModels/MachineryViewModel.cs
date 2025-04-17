using System;
using System.Windows.Input;

namespace DanfossHeating.ViewModels;

public class MachineryViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Machinery;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    public MachineryViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(() => { /* Already on settings page */ });
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        
        Console.WriteLine($"MachineryViewModel created for user: {userName}");
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
    
    private void NavigateToCO2Emission()
    {
        if (MainViewModel != null)
        {
            var viewModel = new CO2EmissionViewModel(UserName, IsDarkTheme);
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
