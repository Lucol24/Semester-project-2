using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore.Measure;
using Avalonia.Animation.Easings;
using LiveChartsCore.Drawing;
using Avalonia;
using Avalonia.Platform.Storage;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace DanfossHeating.ViewModels;

public class OptimizerViewModel : PageViewModelBase
{
    // Static fields to persist settings
    private static string LastSelectedSeason = "Summer";
    private static string LastSelectedScenario = "Scenario 1";
    private static string LastSelectedOptimizationCriteria = "Cost";
    private static int LastOptimizationCriteriaIndex = 0;
    private static bool LastIsZooming = false;

    public override PageType PageType => PageType.Optimizer;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToCostCommand { get; }
    public ICommand NavigateToCO2EmissionCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    public ICommand OptimizeCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand ToggleControlsVisibilityCommand { get; }
    public ICommand DismissControlsNotificationCommand { get; }    public ICommand ExportChartCommand { get; }
    public ICommand ShowElectricityPricesCommand { get; }
    public ICommand ShowProductionCostsCommand { get; }
    public ICommand ShowCO2EmissionsCommand { get; }
    
    private string _selectedSeason;// Remove default value
    private string _selectedScenario; // Remove default value
    private string _selectedOptimizationCriteria; // Remove default value
    private int _optimizationCriteriaIndex; // Remove default value
    private bool _isControlPanelVisible = true; // Default to visible
    private Easing _easingFunction = new CubicEaseOut(); // Default easing function
    private bool _showControlsNotification = true; // Show notification by default
    private string _toggleButtonTooltip = "Click to hide control panel"; // Plain tooltip

    private double _buttonMaxWidth = 775;
    public double ButtonMaxWidth
    {
        get => _buttonMaxWidth;
        set
        {
            if (_buttonMaxWidth != value)
            {
                _buttonMaxWidth = value;
                OnPropertyChanged(nameof(ButtonMaxWidth));
            }
        }
    }

    private bool _isCompactMode;
    public bool IsCompactMode
    {
        get => _isCompactMode;
        set
        {
            if (_isCompactMode != value)
            {
                _isCompactMode = value;
                OnPropertyChanged(nameof(IsCompactMode));
                // You can add responsive layout logic here if needed
            }
        }
    }    private bool _showingElectricityPrices = false;
    public bool ShowingElectricityPrices
    {
        get => _showingElectricityPrices;
        private set
        {
            if (_showingElectricityPrices != value)
            {
                _showingElectricityPrices = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _showingCO2Emissions = false; 
    public bool ShowingCO2Emissions
    {
        get => _showingCO2Emissions;
        private set
        {
            if (_showingCO2Emissions != value)
            {
                _showingCO2Emissions = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _showingProductionCosts = false;
    public bool ShowingProductionCosts
    {
        get => _showingProductionCosts;
        private set
        {
            if (_showingProductionCosts != value)
            {
                _showingProductionCosts = value;
                OnPropertyChanged();
            }
        }
    }

    // Add new property for binding to ComboBox SelectedIndex instead of SelectedItem
    public int OptimizationCriteriaIndex
    {
        get { return _optimizationCriteriaIndex; }
        set
        {
            if (_optimizationCriteriaIndex != value)
            {
                _optimizationCriteriaIndex = value;
                LastOptimizationCriteriaIndex = value; // Save the setting
                // Convert index to actual string value
                _selectedOptimizationCriteria = value == 0 ? "Cost" : "CO2";
                LastSelectedOptimizationCriteria = _selectedOptimizationCriteria; // Save the criteria too
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedOptimizationCriteria)); // Also notify this property changed
                UpdateChartTitle();
                Console.WriteLine($"OptimizationCriteriaIndex set to: {value}, criteria: {_selectedOptimizationCriteria}");
            }
        }
    }

    public string SelectedOptimizationCriteria
    {
        get { return _selectedOptimizationCriteria; }
        set
        {
            if (_selectedOptimizationCriteria != value)
            {
                // Convert direct string value to index
                if (value == "CO2")
                {
                    _optimizationCriteriaIndex = 1;
                    _selectedOptimizationCriteria = "CO2";
                }
                else
                {
                    _optimizationCriteriaIndex = 0;
                    _selectedOptimizationCriteria = "Cost";
                }
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(OptimizationCriteriaIndex));
                UpdateChartTitle();
                Console.WriteLine($"SelectedOptimizationCriteria set to: {_selectedOptimizationCriteria}");
                
                // Do not optimize automatically on criteria change
            }
        }
    }

    // Control panel visibility property
    public bool IsControlPanelVisible
    {
        get { return _isControlPanelVisible; }
        set
        {
            if (_isControlPanelVisible != value)
            {
                _isControlPanelVisible = value;
                OnPropertyChanged();
                Console.WriteLine($"Control panel visibility set to: {value}");
            }
        }
    }

    // Control panel notification property
    public bool ShowControlsNotification
    {
        get { return _showControlsNotification; }
        set
        {
            if (_showControlsNotification != value)
            {
                _showControlsNotification = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Enhanced tooltip for toggle button
    public string ToggleButtonTooltip
    {
        get { return _toggleButtonTooltip; }
        set
        {
            if (_toggleButtonTooltip != value)
            {
                _toggleButtonTooltip = value;
                OnPropertyChanged();
            }
        }
    }

    // Plain season options
    public List<string> SeasonOptions { get; } =
    [
        "Summer", 
        "Winter" 
    ];

    // Plain scenario options
    public List<string> ScenarioOptions { get; } =
    [
        "Scenario 1", 
        "Scenario 2" 
    ];

    // Plain optimization criteria options
    public List<string> OptimizationOptions { get; } =
    [
        "Cost", 
        "CO2" 
    ];

    // Animation easing function property
    public Easing EasingFunction
    {
        get { return _easingFunction; }
        set
        {
            if (_easingFunction != value)
            {
                _easingFunction = value;
                OnPropertyChanged();
            }
        }
    }

    // Chart data and settings
    public ISeries[] Series { get; private set; } = [];
    public Axis[] XAxes { get; set; } =
    [
        new() {
            Name = "Time",
            NameTextSize = 40, // Increase from default (much larger title)
            NamePaint = new SolidColorPaint(SKColors.Gray),
            LabelsPaint = new SolidColorPaint(SKColors.Gray),
            TextSize = 14, // Increase label text size from default (typically 11)
        }
    ];
    public Axis[] YAxes { get; set; } =
    [
        new() {
            Name = "Energy Production (kWh)",
            NameTextSize = 20, // Increase from default (much larger title)
            NamePaint = new SolidColorPaint(SKColors.Gray),
            LabelsPaint = new SolidColorPaint(SKColors.Gray),
            TextSize = 20, // Increase label text size
        }
    ];
    
    // Chart design elements - Initialize paints here
    public LabelVisual Title { get; set; } = new LabelVisual { Paint = new SolidColorPaint(), TextSize = 25, Padding = new LiveChartsCore.Drawing.Padding(15) };
    public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint();
    public SolidColorPaint TooltipBackgroundPaint { get; set; } = new SolidColorPaint();
    public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint { StrokeThickness = 1.2f };
    public SolidColorPaint LegendBackgroundPaint { get; set; } = new SolidColorPaint { StrokeThickness = 1.0f };
    
    // Private fields for data management
    private Optimizer optimizer;
    private AssetManager assetManager;
    private SourceDataManager sourceDataManager;
    private ResultDataManager resultDataManager;
    private double _chartHeight = 500;
    private double _chartWidth = 800;
    private bool _isZooming = false;
    private CartesianChart? _chartControl;

    private string? _exportSuccessMessage;
    private bool _showExportSuccessNotification;

    public string? ExportSuccessMessage
    {
        get => _exportSuccessMessage;
        set => SetProperty(ref _exportSuccessMessage, value);
    }

    public bool ShowExportSuccessNotification
    {
        get => _showExportSuccessNotification;
        set => SetProperty(ref _showExportSuccessNotification, value);
    }        public string SelectedSeason
        {
            get { return _selectedSeason; }
            set
            {
                if (_selectedSeason != value)
                {
                    _selectedSeason = value;
                    LastSelectedSeason = value; // Save the setting
                    OnPropertyChanged();
                    Console.WriteLine($"SelectedSeason set to: {value}");
                }
            }
        }        public string SelectedScenario
        {
            get { return _selectedScenario; }
            set
            {
                if (_selectedScenario != value)
                {
                    _selectedScenario = value;
                    LastSelectedScenario = value; // Save the setting
                    OnPropertyChanged();
                    Console.WriteLine($"SelectedScenario set to: {value}");
                }
            }
        }

    public double ChartHeight
    {
        get { return _chartHeight; }
        set
        {
            if (_chartHeight != value)
            {
                _chartHeight = value;
                OnPropertyChanged();
            }
        }
    }

    public double ChartWidth
    {
        get { return _chartWidth; }
        set
        {
            if (_chartWidth != value)
            {
                _chartWidth = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsZooming
    {
        get { return _isZooming; }
        set
        {
            if (_isZooming != value)
            {
                _isZooming = value;
                LastIsZooming = value; // Save the setting
                OnPropertyChanged();
            }
        }
    }
      public OptimizerViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        // Initialize fields with last used values
        _selectedSeason = LastSelectedSeason;
        _selectedScenario = LastSelectedScenario;
        _selectedOptimizationCriteria = LastSelectedOptimizationCriteria;
        _optimizationCriteriaIndex = LastOptimizationCriteriaIndex;
        _isZooming = LastIsZooming;

        // Initialize Commands

        NavigateToHomeCommand = new Command(NavigateToHome);
        NavigateToOptimizerCommand = new Command(() => { /* Already on optimizer page */ });
        NavigateToCostCommand = new Command(NavigateToCost);
        NavigateToCO2EmissionCommand = new Command(NavigateToCO2Emission);
        NavigateToMachineryCommand = new Command(NavigateToMachinery);        NavigateToAboutUsCommand = new Command(NavigateToAboutUs);
        OptimizeCommand = new RelayCommand(OptimizeData);
        ResetZoomCommand = new RelayCommand(ResetZoom);
        ToggleControlsVisibilityCommand = new RelayCommand(ToggleControlsVisibility);
        DismissControlsNotificationCommand = new RelayCommand(DismissControlsNotification);
        ExportChartCommand = new RelayCommand(ExportChart);
        ShowElectricityPricesCommand = new RelayCommand(ShowElectricityPrices);
        ShowProductionCostsCommand = new RelayCommand(ShowProductionCosts);
        ShowCO2EmissionsCommand = new RelayCommand(ShowCO2Emissions);
        
        // Make sure the controls notification is shown by default
        _showControlsNotification = true;
        
        // Initialize managers and optimizer
        assetManager = new AssetManager();
        sourceDataManager = new SourceDataManager();
        resultDataManager = new ResultDataManager();
        optimizer = new Optimizer(assetManager, sourceDataManager, resultDataManager);
        
        // Initialize chart visuals
        InitializeChartElements();
        
        // Trigger optimization with loaded values
        OptimizeData();
        
        Console.WriteLine($"OptimizerViewModel created for user: {userName} with last settings: Season={_selectedSeason}, Scenario={_selectedScenario}, Criteria={_selectedOptimizationCriteria}");
    }
    
    private void InitializeChartElements()
    {
        // Set initial text for Title
        Title.Text = "Heat Production Optimization"; 
        
        // Apply initial theme settings to the existing paint objects
        UpdateChartThemeProperties(); 
    }

    private void UpdateChartTitle()
    {
        if (Title?.Paint is SolidColorPaint titlePaint)
        {
            string criteriaText = _selectedOptimizationCriteria;
            Title.Text = $"Heat Production - {criteriaText} Optimization";
            titlePaint.Color = IsDarkTheme ? SKColors.White : SKColors.Black;
            Console.WriteLine($"Updated chart title to: {Title.Text}");
            OnPropertyChanged(nameof(Title));
        }
    }
    
    private void LoadChart()
    {
        _showingElectricityPrices = false;

        try
        {
            var results = resultDataManager.LoadResults();

            if (results == null || results.Count == 0)
            {
                Console.WriteLine("No results data available to display");
                // Set up empty chart
                Series = [];
                XAxes = [new Axis { Name = "Time" }];
                YAxes = [new Axis { Name = "Heat Produced (MWh)" }];
                return;
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

            // Create labels for the x-axis with proper hour formatting
            var allLabels = groupedByHour.Keys
                .Select(k => {
                    // Format with proper hours (not just 00:00)
                    return $"{k.Date.ToString("dd/MM/yyyy")} {k.Hour:D2}:00";
                })
                .ToArray();
                
            Console.WriteLine($"Sample timestamp format: {allLabels.FirstOrDefault()}");
            
            var labelCount = allLabels.Length;
            var skipFactor = Math.Max(1, labelCount / 8); // Show at most 8 labels when zoomed out
            
            // Initially set empty labels for non-important timestamps to avoid cluttering
            var labels = allLabels
                .Select((label, index) => (index % skipFactor == 0) ? label : string.Empty)
                .ToArray();

            // Colors for the series with improved palette
            var colors = new[]
            {
                SKColors.MediumSeaGreen, // Green
                SKColors.DodgerBlue,     // Blue
                SKColors.Orange,         // Orange
                SKColors.HotPink,        // Pink
                SKColors.MediumPurple,   // Purple
                SKColors.LightSeaGreen   // Cyan
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

                // Check if this machine has any non-zero production values
                bool hasNonZeroProduction = values.Any(v => Math.Abs(v) > 0.001); // Using small epsilon to account for floating point precision

                // Get the max value for this series to intelligently display labels
                double maxValue = values.Max();
                double valueThreshold = maxValue * 0.15; // Only show labels for values > 15% of max

                seriesList.Add(new StackedColumnSeries<double>
                {
                    Values = values,
                    Name = unit,
                    Fill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length].WithAlpha(200)) { StrokeThickness = 1 },
                    Padding = 4, // Increase padding between columns
                    MaxBarWidth = 35, // Slightly wider bars
                    IsVisibleAtLegend = hasNonZeroProduction, // Only show in legend if it has production
                    // Remove data labels from the bars - values will only appear in tooltips
                    DataLabelsFormatter = (point) => string.Empty, // Always return empty string to hide labels
                    DataLabelsPaint = new SolidColorPaint(
                        IsDarkTheme ? 
                        SKColors.White.WithAlpha(240) : 
                        SKColors.Black.WithAlpha(220)
                    )
                });

                colorIndex++;
            }

            Series = seriesList.ToArray();

            // Define axis paints based on the current theme *before* creating axes
            var axisLabelPaint = new SolidColorPaint(IsDarkTheme ? SKColors.White : SKColors.Black);
            var axisSeparatorPaint = new SolidColorPaint(IsDarkTheme ? SKColor.Parse("#404040") : SKColor.Parse("#DFDFDF")) { StrokeThickness = 0.8f };
            var axisTicksPaint = new SolidColorPaint(IsDarkTheme ? SKColor.Parse("#808080") : SKColor.Parse("#A0A0A0"));
            var axisNamePaint = new SolidColorPaint(IsDarkTheme ? SKColors.White : SKColors.Black); // Use same color as labels for name

            // Enhanced X-axis configuration using the defined paints
            XAxes = new Axis[]
            {
                new Axis
                {                    Labels = allLabels, 
                    LabelsRotation = 45,
                    MinStep = 1, 
                    ForceStepToMin = false, 
                    Padding = new Padding(5, 20, 5, 0), 
                    TextSize = 11, // Match insights charts text size
                    SeparatorsPaint = axisSeparatorPaint, // Apply theme paint
                    TicksPaint = axisTicksPaint,          // Apply theme paint
                    LabelsPaint = axisLabelPaint,         // Apply theme paint
                    Labeler = (value) => {
                        int index = (int)value;
                        if (index < 0 || index >= allLabels.Length) return string.Empty;
                        return allLabels[index];
                    },
                    MinLimit = -1,
                    MaxLimit = allLabels.Length,
                    Name = "Time",  
                    NamePaint = axisNamePaint,            // Apply theme paint
                    NameTextSize = 20
                }
            };

            // Enhanced Y-axis configuration using the defined paints
            YAxes =
            [
                new() {
                    Name = "Heat Produced (MWh)",
                    NamePaint = axisNamePaint,            // Apply theme paint
                    NameTextSize = 20,
                    MinLimit = 0,
                    ShowSeparatorLines = true,
                    SeparatorsPaint = axisSeparatorPaint, // Apply theme paint
                    TextSize = 12, 
                    TicksPaint = axisTicksPaint,          // Apply theme paint
                    LabelsPaint = axisLabelPaint,         // Apply theme paint
                    Labeler = (value) => value.ToString("N2") 
                }
            ];

            // Update the chart title (which also updates its paint color)
            UpdateChartTitle();

            Console.WriteLine($"Loaded {results.Count} results");
            Console.WriteLine($"Created {seriesList.Count} series");
            
            // Log which machines have zero production
            var hiddenMachines = seriesList
                .OfType<StackedColumnSeries<double>>()
                .Where(s => !s.IsVisibleAtLegend)
                .Select(s => s.Name)
                .ToList();
                
            if (hiddenMachines.Any())
            {
                Console.WriteLine($"Hiding {hiddenMachines.Count} machines with zero production from legend: {string.Join(", ", hiddenMachines)}");
            }
            
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading chart: {ex.Message}");
        }
    }

    private void OptimizeData()
    {
        try
        {
            OptimizationCriteria criteria = _selectedOptimizationCriteria == "CO2" ? 
                OptimizationCriteria.CO2Emissions : 
                OptimizationCriteria.Cost;
                
            bool useScenario2 = SelectedScenario == "Scenario 2";
            
            optimizer.OptimizeHeatProduction(SelectedSeason, criteria, useScenario2);
            Console.WriteLine($"Optimizing data for season: {SelectedSeason}, criteria: {criteria}, scenario2: {useScenario2}");

            // Reload the chart with the optimized data
            LoadChart();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OptimizeData: {ex}");
        }
    }
    
    private void ResetZoom()
    {
        // Signal to the chart to reset its zoom
        IsZooming = false; 
        LoadChart(); // Reloading might reset zoom implicitly depending on chart setup
        OnPropertyChanged(nameof(Series)); // Notify potentially needed for chart updates
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(YAxes));
    }
    
    private void ToggleControlsVisibility()
    {
        IsControlPanelVisible = !IsControlPanelVisible;
        // Update the tooltip based on current state (plain text)
        ToggleButtonTooltip = IsControlPanelVisible ? 
            "Click to hide control panel" : 
            "Click to show control panel";
        
        Console.WriteLine($"Toggled control panel visibility to: {IsControlPanelVisible}");
        
        // Don't hide the notification when toggling - let the user explicitly dismiss it
    }
    
    private void DismissControlsNotification()
    {
        ShowControlsNotification = false;
        Console.WriteLine("Controls notification dismissed");
    }
    
    public void SetChartControl(CartesianChart chart)
    {
        _chartControl = chart;
        Console.WriteLine("Chart control reference set in ViewModel");
    }

    private async void ExportChart()
    {
        try
        {
            if (_chartControl == null)
            {
                Console.WriteLine("Chart control reference not found");
                return;
            }

            var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = app?.MainWindow;
            if (mainWindow == null)
            {
                Console.WriteLine("Window not found");
                return;
            }

            // Create file picker options
            var options = new FilePickerSaveOptions
            {
                Title = "Save Chart",
                DefaultExtension = ".png",
                ShowOverwritePrompt = true,
                FileTypeChoices =
                [
                    new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } }
                ],
                SuggestedFileName = $"HeatProduction_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            // Show save dialog
            var file = await mainWindow.StorageProvider.SaveFilePickerAsync(options);
            if (file == null) return;

            // Create a bitmap with increased height (1.05x taller)
            var pixelSize = new PixelSize(
                (int)_chartControl.Bounds.Width,
                (int)(_chartControl.Bounds.Height * 1.05) // Make the output taller
            );

            // Ensure we're on the UI thread
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try 
                {
                    using (var bitmap = new RenderTargetBitmap(pixelSize))
                    {
                        // First measure the control with the increased height
                        _chartControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        
                        // Then arrange it with the increased height
                        var size = new Size(_chartControl.Bounds.Width, _chartControl.Bounds.Height * 1.05);
                        _chartControl.Arrange(new Rect(new Point(0, 0), size));
                        
                        // Render the chart to the bitmap
                        bitmap.Render(_chartControl);

                        // Save the bitmap as PNG
                        using var stream = await file.OpenWriteAsync();
                        bitmap.Save(stream);
                    }
                    Console.WriteLine($"Chart exported successfully to: {file.Path.LocalPath}");
                    
                    // Show success notification
                    ExportSuccessMessage = $"Chart exported successfully to {file.Name}!";
                    ShowExportSuccessNotification = true;

                    // Auto-hide notification after 4 seconds
                    await Task.Delay(4000);
                    ShowExportSuccessNotification = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during chart rendering/saving: {ex.Message}");
                    ExportSuccessMessage = "Error exporting chart. Please try again.";
                    ShowExportSuccessNotification = true;
                    await Task.Delay(4000);
                    ShowExportSuccessNotification = false;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExportChart: {ex.Message}");
            ExportSuccessMessage = "Error exporting chart. Please try again.";
            ShowExportSuccessNotification = true;
            await Task.Delay(4000);
            ShowExportSuccessNotification = false;
        }
    }      private void ShowElectricityPrices()
    {
        ShowingElectricityPrices = true;
        ShowingProductionCosts = false;
        ShowingCO2Emissions = false;

        // Get the heat demands for the current season and extract electricity prices
        var demands = _selectedSeason.ToLower() == "summer" 
            ? sourceDataManager.GetSummerHeatDemands()
            : sourceDataManager.GetWinterHeatDemands();
            
        var times = demands.Select(d => d.TimeFrom.ToString("HH:00")).ToArray();
        var prices = demands.Select(d => d.ElectricityPrice).ToArray();

        // Define theme-aware colors
        var priceLineColor = IsDarkTheme ? SKColors.DodgerBlue : SKColor.Parse("#0078D4");
        var fillColor = IsDarkTheme ? 
            SKColor.Parse("#200078D4") : // Darker theme: lighter blue with transparency
            SKColor.Parse("#150078D4");  // Light theme: slightly more transparent
        var axisLabelColor = IsDarkTheme ? SKColors.White : SKColors.Black;
        var axisSeparatorColor = IsDarkTheme ? SKColor.Parse("#404040") : SKColor.Parse("#DFDFDF");
        var pointColor = SKColors.Red; // Changed to red

        // Create series for electricity prices
        Series =
        [
            new LineSeries<double>
            {
                Name = "Electricity Price",
                Values = prices,                Stroke = new SolidColorPaint(priceLineColor) { StrokeThickness = 2.5f },
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 1.5f },
                GeometrySize = 8,
                Fill = new SolidColorPaint(fillColor),
                LineSmoothness = 0.2 // Subtle curve that maintains data accuracy
            }
        ];

        // Enhanced X-axis configuration
        XAxes =
        [
            new() {
                Name = "Date and Time",
                NameTextSize = 20, // Increased to match optimizer
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                Labels = demands.Select(d => d.TimeFrom.ToString("dd/MM HH:00")).ToArray(), // Shorter date format
                LabelsRotation = 45, // Angled for better readability
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = Math.Max(1, demands.Count() / 12), // Show roughly 12 labels
                Padding = new Padding(5)
            }
        ];

        // Enhanced Y-axis configuration
        YAxes =
        [
            new() {
                Name = "Price (DKK/kWh)",
                NameTextSize = 20, // Increased to match optimizer
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                TextSize = 11,
                Labeler = (value) => $"{value:F2}",
                MinLimit = 0,
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = 0.1, // Ensure reasonable price increments
                Padding = new Padding(5)
            }
        ];

        // Enhanced title with theme-aware color
        Title = new LabelVisual
        {
            Text = $"Electricity Prices - {_selectedSeason} {_selectedScenario}",
            TextSize = 20,
            Paint = new SolidColorPaint(axisLabelColor),
            Padding = new Padding(15)
        };
        
        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(YAxes));
        OnPropertyChanged(nameof(Title));
    }    private void ShowProductionCosts() 
    {
        ShowingElectricityPrices = false;
        ShowingProductionCosts = true;
        ShowingCO2Emissions = false;

        // First optimize data for current settings
        OptimizeData();

        // Then update chart with production costs
        var results = resultDataManager.LoadResults();
        if (results == null || results.Count == 0)
        {
            Series = Array.Empty<ISeries>();
            UpdateChartTitle();
            return;
        }

        // Group data by unit name and time
        var groupedByTime = results
            .GroupBy(r => r.Timestamp)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Extract all timestamps and create labels
        var timestamps = groupedByTime.Keys.ToList();
        var labels = timestamps.Select(t => t.ToString("dd/MM HH:00")).ToArray();

        // Define theme-aware colors
        var axisLabelColor = IsDarkTheme ? SKColors.White : SKColors.Black;
        var axisSeparatorColor = IsDarkTheme ? SKColor.Parse("#404040") : SKColor.Parse("#DFDFDF");

        var colors = new[]
        {
            SKColors.MediumSeaGreen,
            SKColors.DodgerBlue,
            SKColors.Orange,
            SKColors.HotPink
        };

        // Create series for each unit's production costs
        var seriesList = new List<ISeries>();
        var uniqueUnits = results.Select(r => r.UnitName).Distinct().ToList();
        int colorIndex = 0;

        foreach (var unit in uniqueUnits)
        {            var values = timestamps.Select(time =>
                groupedByTime[time]
                    .Where(r => r.UnitName == unit)
                    .Select(r => -r.ProductionCost) // Invert values so costs are negative (going down) and earnings are positive (going up)
                    .DefaultIfEmpty(0)
                    .First()
            ).ToArray();

            if (values.Any(v => v != 0)) // Show if there are any non-zero values
            {
                // Calculate the threshold dynamically based on zoom state and data
                var maxValue = values.Max();
                var thresholdPercentage = IsZooming ? 0.15 : 0.55; // Show fewer labels when not zoomed
                var threshold = maxValue * thresholdPercentage;

                // Add a StackedColumnSeries for better visual consistency
                seriesList.Add(new StackedColumnSeries<double>
                {
                    Name = $"{unit}",
                    Values = values,
                    Fill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length].WithAlpha(200)) { StrokeThickness = 1 },
                    Padding = 4,
                    MaxBarWidth = 35,
                    IsVisibleAtLegend = true,                    DataLabelsFormatter = (point) => string.Empty, // No labels above bars
                    DataLabelsPaint = new SolidColorPaint(
                        IsDarkTheme ? SKColors.White.WithAlpha(240) : SKColors.Black.WithAlpha(220)
                    )
                });
                colorIndex++;
            }
        }

        Series = seriesList.ToArray();

        // Enhanced X-axis configuration
        XAxes =
        [
            new() {
                Name = "Time",
                NameTextSize = 20,
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                Labels = labels,
                LabelsRotation = 45,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = Math.Max(1, labels.Length / 12),
                Padding = new Padding(5)
            }
        ];        // Enhanced Y-axis configuration
        YAxes =
        [
            new() {
                Name = "Production Cost (DKK)",
                NameTextSize = 20,
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                TextSize = 11,
                Labeler = (value) => $"{value:F0}",
                // Remove MinLimit to allow negative values
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = 100,
                Position = AxisPosition.Start,
                Padding = new Padding(5)
            }
        ];

        // Enhanced title with theme-aware color
        Title = new LabelVisual
        {
            Text = $"Production Costs - {_selectedSeason} {_selectedScenario}",
            TextSize = 20,
            Paint = new SolidColorPaint(axisLabelColor),
            Padding = new Padding(15)
        };

        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(YAxes));
        OnPropertyChanged(nameof(Title));
    }
      private void ShowCO2Emissions()
    {
        ShowingElectricityPrices = false;
        ShowingProductionCosts = false;
        ShowingCO2Emissions = true;

        // First optimize data for current settings
        OptimizeData();

        // Then update chart with CO2 emissions
        var results = resultDataManager.LoadResults();
        if (results == null || results.Count == 0)
        {
            Series = Array.Empty<ISeries>();
            UpdateChartTitle();
            return;
        }

        // Group data by unit name and time
        var groupedByTime = results
            .GroupBy(r => r.Timestamp)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Extract all timestamps and create labels
        var timestamps = groupedByTime.Keys.ToList();
        var labels = timestamps.Select(t => t.ToString("dd/MM HH:00")).ToArray();

        // Define theme-aware colors
        var axisLabelColor = IsDarkTheme ? SKColors.White : SKColors.Black;
        var axisSeparatorColor = IsDarkTheme ? SKColor.Parse("#404040") : SKColor.Parse("#DFDFDF");        var colors = new[]
        {
            SKColors.MediumSeaGreen,  // Green for biomass/sustainable
            SKColor.Parse("#1E88E5"),  // Blue for clean energy
            SKColor.Parse("#FFA726"),  // Orange for mixed sources
            SKColor.Parse("#D81B60")   // Pink/Red for high emissions
        };

        // Create series for each unit's CO2 emissions
        var seriesList = new List<ISeries>();
        var uniqueUnits = results.Select(r => r.UnitName).Distinct().ToList();
        int colorIndex = 0;

        foreach (var unit in uniqueUnits)
        {
            var values = timestamps.Select(time =>
                groupedByTime[time]
                    .Where(r => r.UnitName == unit)
                    .Select(r => r.CO2Emissions) // CO2 emissions are already in the right direction
                    .DefaultIfEmpty(0)
                    .First()
            ).ToArray();

            if (values.Any(v => v != 0)) // Show if there are any non-zero values
            {
                // Calculate the threshold dynamically based on zoom state and data
                var maxValue = values.Max();
                var thresholdPercentage = IsZooming ? 0.15 : 0.55; // Show fewer labels when not zoomed
                var threshold = maxValue * thresholdPercentage;

                // Add a StackedColumnSeries for better visual consistency
                seriesList.Add(new StackedColumnSeries<double>
                {
                    Name = $"{unit}",
                    Values = values,
                    Fill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length].WithAlpha(200)) { StrokeThickness = 1 },
                    Padding = 4,
                    MaxBarWidth = 35,
                    IsVisibleAtLegend = true,
                    DataLabelsFormatter = (point) => string.Empty, // No labels above bars
                    DataLabelsPaint = new SolidColorPaint(
                        IsDarkTheme ? SKColors.White.WithAlpha(240) : SKColors.Black.WithAlpha(220)
                    )
                });
                colorIndex++;
            }
        }

        Series = seriesList.ToArray();

        // Enhanced X-axis configuration
        XAxes =
        [
            new() {
                Name = "Time",
                NameTextSize = 20,
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                Labels = labels,
                LabelsRotation = 45,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = Math.Max(1, labels.Length / 12),
                Padding = new Padding(5)
            }
        ];

        // Enhanced Y-axis configuration
        YAxes =
        [
            new() {
                Name = "CO₂ Emissions (kg)",
                NameTextSize = 20,
                NamePaint = new SolidColorPaint(axisLabelColor) { StrokeThickness = 1 },
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                TextSize = 11,
                Labeler = (value) => $"{value:F0}",
                MinLimit = 0,
                SeparatorsPaint = new SolidColorPaint(axisSeparatorColor) { StrokeThickness = 0.5f },
                ShowSeparatorLines = true,
                MinStep = 50,
                Position = AxisPosition.Start,
                Padding = new Padding(5)
            }
        ];

        // Enhanced title with theme-aware color
        Title = new LabelVisual
        {
            Text = $"CO₂ Emissions - {_selectedSeason} {_selectedScenario}",
            TextSize = 20,
            Paint = new SolidColorPaint(axisLabelColor),
            Padding = new Padding(15)
        };

        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(YAxes));
        OnPropertyChanged(nameof(Title));
    }
    
    // Navigation methods remain the same
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
        // Instead of navigating to cost page, set optimization criteria to Cost
        SelectedOptimizationCriteria = "Cost";
    }
    
    private void NavigateToCO2Emission()
    {
        // Instead of navigating to CO2 page, set optimization criteria to CO2
        SelectedOptimizationCriteria = "CO2";
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

    private void UpdateChartTheme()
    {
        // Update all theme-dependent visual properties by modifying existing paint objects
        UpdateChartThemeProperties(); 
        
        // Reload the appropriate chart based on current view
        if (_showingElectricityPrices)
        {
            ShowElectricityPrices();
        }
        else if (_showingCO2Emissions)
        {
            ShowCO2Emissions();
        }
        else
        {
            ShowProductionCosts();
        }
    }

    // Helper method to update all theme-dependent paint properties
    private void UpdateChartThemeProperties()
    {
        // Title Paint
        if (Title?.Paint is SolidColorPaint titlePaint)
        {
            titlePaint.Color = IsDarkTheme ? SKColors.White : SKColors.Black;
        }

        // Tooltip Paints
        TooltipTextPaint.Color = IsDarkTheme ? SKColors.White : SKColors.Black;
        var tooltipBgColor = IsDarkTheme ? SKColor.Parse("#2D3035") : SKColors.White;
        TooltipBackgroundPaint.Color = tooltipBgColor.WithAlpha(230);

        // Legend Paints
        LegendTextPaint.Color = IsDarkTheme ? SKColor.Parse("#EEEEEE") : SKColors.Black;
        var legendBgColor = IsDarkTheme ? SKColor.Parse("#2D3035") : SKColor.Parse("#FCFCFC");
        LegendBackgroundPaint.Color = legendBgColor.WithAlpha(240);

        // Axis Paints (Update existing axes if they exist)
        var axisLabelColor = IsDarkTheme ? SKColors.White : SKColors.Black;
        var axisSeparatorColor = IsDarkTheme ? SKColor.Parse("#404040") : SKColor.Parse("#DFDFDF");
        var axisTicksColor = IsDarkTheme ? SKColor.Parse("#808080") : SKColor.Parse("#A0A0A0");
        var axisNameColor = axisLabelColor; // Use same color for name

        // Update paints directly in the loops with null checks
        if (XAxes != null)
        {
            foreach (var axis in XAxes)
            {
                if (axis != null) // Ensure axis is not null
                {
                    if (axis.LabelsPaint is SolidColorPaint labelPaint)
                    {
                        labelPaint.Color = axisLabelColor;
                    }
                    if (axis.NamePaint is SolidColorPaint namePaint)
                    {
                        namePaint.Color = axisNameColor;
                    }
                    if (axis.SeparatorsPaint is SolidColorPaint separatorPaint)
                    {
                        separatorPaint.Color = axisSeparatorColor;
                    }
                    if (axis.TicksPaint is SolidColorPaint ticksPaint)
                    {
                        ticksPaint.Color = axisTicksColor;
                    }
                }
            }
        }

        if (YAxes != null)
        {
            foreach (var axis in YAxes)
            {
                if (axis != null) // Ensure axis is not null
                {
                    if (axis.LabelsPaint is SolidColorPaint labelPaint)
                    {
                        labelPaint.Color = axisLabelColor;
                    }
                    if (axis.NamePaint is SolidColorPaint namePaint)
                    {
                        namePaint.Color = axisNameColor;
                    }
                    if (axis.SeparatorsPaint is SolidColorPaint separatorPaint)
                    {
                        separatorPaint.Color = axisSeparatorColor;
                    }
                    if (axis.TicksPaint is SolidColorPaint ticksPaint)
                    {
                        ticksPaint.Color = axisTicksColor;
                    }
                }
            }
        }

        // Notify that paint properties have changed
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(TooltipTextPaint));
        OnPropertyChanged(nameof(TooltipBackgroundPaint));
        OnPropertyChanged(nameof(LegendTextPaint));
        OnPropertyChanged(nameof(LegendBackgroundPaint));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(YAxes));
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        
        // Update chart theme when IsDarkTheme property changes
        if (propertyName == nameof(IsDarkTheme))
        {
            UpdateChartTheme();
        }
    }
}
