using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace DanfossHeating.ViewModels;
public class HomePageViewModel : PageViewModelBase
{
    private Bitmap? _profileImage;
    private string _currentDate = string.Empty;

    public Bitmap? ProfileImage
    {
        get => _profileImage;
        set => SetProperty(ref _profileImage, value);
    }

    public string CurrentDate
    {
        get => _currentDate;
        private set => SetProperty(ref _currentDate, value);
    }

    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }

    public override PageType PageType => PageType.Home;

    public HomePageViewModel() : base("User", false)
    {
        // Initialize navigation commands
        NavigateToHomeCommand = new Command(() => { /* Already on home page */ });
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);

        UpdateCurrentDate();
        StartDateUpdateTimer();
    }

    public HomePageViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        Console.WriteLine($"Creating HomePageViewModel with userName: {userName}, isDarkTheme: {isDarkTheme}");
        
        // Initialize navigation commands
        NavigateToHomeCommand = new Command(() => { /* Already on home page */ });
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);

        UpdateCurrentDate();
        StartDateUpdateTimer();
    }

    private void UpdateCurrentDate()
    {
        CurrentDate = DateTime.Now.ToString("dddd, MMMM d, yyyy");
    }

    private void StartDateUpdateTimer()
    {
        // Calculate time until next midnight
        var now = DateTime.Now;
        var tomorrow = now.AddDays(1).Date;
        var timeUntilMidnight = tomorrow - now;

        // Use a timer to update the date at midnight
        System.Threading.Tasks.Task.Delay(timeUntilMidnight).ContinueWith(_ =>
        {
            UpdateCurrentDate();
            StartDateUpdateTimer();
        });
    }

    private void NavigateToOptimizer()
    {
        if (MainViewModel != null)
        {
            var optimizerViewModel = new OptimizerViewModel(UserName, IsDarkTheme);
            optimizerViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(optimizerViewModel);
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

    private void NavigateToMachinery()
    {
        if (MainViewModel != null)
        {
            var machineryViewModel = new MachineryViewModel(UserName, IsDarkTheme);
            machineryViewModel.SetMainViewModel(MainViewModel);
            MainViewModel.NavigateTo(machineryViewModel);
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