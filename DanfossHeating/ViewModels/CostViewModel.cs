using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using LiveChartsCore;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

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
    public ICommand OptimizeCommand { get; }

    private string _selectedSeason = ""; // Default value
    private string _selectedScenario = ""; // Default value

    public string SelectedSeason
    {
        get { return _selectedSeason; }
        set
        {
            if (_selectedSeason != value)
            {
                _selectedSeason = value;
                OnPropertyChanged();
                Console.WriteLine($"SelectedSeason set to: {value}");
            }
        }
    }

    public string SelectedScenario
    {
        get { return _selectedScenario; }
        set
        {
            if (_selectedScenario != value)
            {
                _selectedScenario = value;
                OnPropertyChanged();
                Console.WriteLine($"SelectedScenario set to: {value}");
            }
        }
    }

    public ISeries[] Series { get; private set; } = [];
    public Axis[] XAxes { get; private set; } = [];
    public Axis[] YAxes { get; private set; } = [];

    private Optimizer optimizer;
    private AssetManager assetManager;
    private SourceDataManager sourceDataManager;
    private ResultDataManager resultDataManager;

    public CostViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        OptimizeCommand = new RelayCommand(OptimizeData);
        Console.WriteLine($"CostViewModel created for user: {userName}");

        // Initialize the AssetManager, SourceDataManager, and ResultDataManager
        assetManager = new AssetManager();
        sourceDataManager = new SourceDataManager();
        resultDataManager = new ResultDataManager();
        optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);

        LoadChart();
    }

    private void LoadChart()
    {
        if (resultDataManager == null)
        {
            throw new InvalidOperationException("resultDataManager is not initialized.");
        }

        var results = resultDataManager.LoadResults();

        if (results == null)
        {
            throw new InvalidOperationException("LoadResults returned null.");
        }

        // Group data by unit name
        var groupedByUnit = results
            .GroupBy(r => r.UnitName)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Timestamp).ToList());

        // Extract all unique timestamps and sort them
        var allTimestamps = results
            .Select(r => r.Timestamp)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        // Group timestamps by hour
        var groupedByHour = allTimestamps
            .GroupBy(t => new { t.Date, t.Hour })
            .OrderBy(g => g.Key.Date)
            .ThenBy(g => g.Key.Hour)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Create labels for the x-axis
        var labels = groupedByHour.Keys.Select(k => k.Date.ToString("dd/MM/yyyy HH:00")).ToArray();

        // Colors for the series
        var colors = new[]
        {
            SKColors.Green,
            SKColors.Blue,
            SKColors.Orange,
            SKColors.Red
        };

        var seriesList = new List<ISeries>();
        int colorIndex = 0;

        foreach (var kvp in groupedByUnit)
        {
            var unit = kvp.Key;
            var unitData = kvp.Value;

            // Map the heat produced values to the corresponding hours
            var values = groupedByHour.Keys.Select(hourKey =>
                unitData.Where(r => r.Timestamp.Date == hourKey.Date && r.Timestamp.Hour == hourKey.Hour)
                        .Sum(r => r.HeatProduced)).ToArray();

            seriesList.Add(new StackedColumnSeries<double>
            {
                Values = values,
                Name = unit,
                Fill = new SolidColorPaint(colors[colorIndex % colors.Length])
            });

            colorIndex++;
        }

        Series = seriesList.ToArray();

        XAxes =
        [
            new() {
                Labels = labels,
                LabelsRotation = 30,
                MinStep = 6, // Set the minimum step to 1 hour
            }
        ];

        YAxes =
        [
            new() {
                Name = "Heat Produced (MWh)",
                MinLimit = 0 // Set the minimum of the Y-axis to zero
            }
        ];

        // Log to verify data loading
        Console.WriteLine($"Loaded {results.Count} results.");
        Console.WriteLine($"Created {seriesList.Count} series.");
    }

    private void OptimizeData()
    {
        try
        {
            if (SelectedScenario == "Scenario 1")
            {
                optimizer.OptimizeHeatProduction(SelectedSeason, OptimizationCriteria.Cost, false);
            }
            else if (SelectedScenario == "Scenario 2")
            {
                optimizer.OptimizeHeatProduction(SelectedSeason, OptimizationCriteria.Cost, true);
            }
            Console.WriteLine($"Optimizing data for season: {SelectedSeason} and is scenario2: {SelectedScenario}");

            // Reload the chart with the optimized data
            LoadChart();
            NavigateToCost(); // Refresh the view after optimization
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OptimizeData: {ex}");
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
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
}
