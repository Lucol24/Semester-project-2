using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
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

    public string[] Labels { get; set; }
    public ISeries[] Series  { get; private set; } = Array.Empty<ISeries>();
    public Axis[] XAxes  { get; private set; } = Array.Empty<Axis>();
    public Axis[] YAxes  { get; private set; } = Array.Empty<Axis>();

    
    public CostViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(NavigateToOptimizer);
        NavigateToCostCommand = new Command(() => { /* Already on cost page */ });
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);
        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        Console.WriteLine($"CostViewModel created for user: {userName}");

        LoadChart();
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

    private void LoadChart()
    {
        ResultDataManager resultDataManager = new ResultDataManager();
        var results = resultDataManager.LoadResults();

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

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 30,
                MinStep = 6, // Set the minimum step to 1 hour
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "Heat Produced (MWh)",
                MinLimit = 0 // Set the minimum of the Y-axis to zero
            }
        };

        // Log to verify data loading
        Console.WriteLine($"Loaded {results.Count} results.");
        Console.WriteLine($"Created {seriesList.Count} series.");
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
