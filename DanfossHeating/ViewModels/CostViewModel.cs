using System;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace DanfossHeating.ViewModels;

public class CostViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Cost;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    public CostViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(() => { /* Already on cost page */ });
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);
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
    
    private void NavigateToMachinery()
    {
        if (MainViewModel != null)
        {
            var viewModel = new MachineryViewModel(UserName, IsDarkTheme);
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

    public ISeries[] Series { get; set; } = new ISeries[]
    {
        new StackedAreaSeries<double> { Values = new double[] { 3, 2, 3, 5, 3, 4, 6 } },
        new StackedAreaSeries<double> { Values = new double[] { 6, 5, 6, 3, 8, 5, 2 } },
        new StackedAreaSeries<double> { Values = new double[] { 4, 8, 2, 8, 9, 5, 3 } }
    };
}
