using System;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace DanfossHeating.ViewModels;

public class MachineryViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Machinery;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    public ObservableCollection<ProductionUnit> Machines { get; set; } = new();
    
    public MachineryViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToMachineryCommand = new Command(() => { /* Already on settings page */ });
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);

        AssetManager assetManager = new AssetManager();
        var units = assetManager.GetProductionUnits();

        for (int i = 0; i < units.Count; i++)
        {
            units[i].ImagePath = $"avares://DanfossHeating/Assets/machine{i + 1}.png";
            Console.WriteLine($"Loaded image path: {units[i].ImagePath}");

        }

        Machines = new ObservableCollection<ProductionUnit>(units);
    
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
