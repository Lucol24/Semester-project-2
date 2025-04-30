using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Data;

namespace DanfossHeating.ViewModels;

public class MachineryViewModel : PageViewModelBase
{
    public override PageType PageType => PageType.Machinery;
    
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToOptimizerCommand { get; }
    public ICommand NavigateToMachineryCommand { get; }
    public ICommand NavigateToAboutUsCommand { get; }
    
    // Add commands for scenario selection
    public ICommand LoadScenario1Command { get; }
    public ICommand LoadScenario2Command { get; }
    
    // Command for dismissing the Danfoss info banner
    public ICommand DismissDanfossInfoCommand { get; }
    
    // Property to track if the Danfoss info banner is visible
    private bool _showDanfossInfoBanner = true;
    public bool ShowDanfossInfoBanner
    {
        get => _showDanfossInfoBanner;
        set
        {
            if (_showDanfossInfoBanner != value)
            {
                _showDanfossInfoBanner = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Track the current scenario state
    private bool _isScenario1Selected = false;
    private bool _isScenario2Selected = false;
    private HashSet<string> _disabledMachines = new HashSet<string>();
    
    public HashSet<string> DisabledMachines
    {
        get => _disabledMachines;
        set
        {
            if (_disabledMachines != value)
            {
                _disabledMachines = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsScenario1Selected
    {
        get => _isScenario1Selected;
        set
        {
            if (_isScenario1Selected != value)
            {
                _isScenario1Selected = value;
                OnPropertyChanged();
                
                // If selecting Scenario 1, deselect Scenario 2
                if (value && _isScenario2Selected)
                {
                    IsScenario2Selected = false;
                }
                
                if (value)
                {
                    LoadScenario1();
                }
            }
        }
    }
    
    public bool IsScenario2Selected
    {
        get => _isScenario2Selected;
        set
        {
            if (_isScenario2Selected != value)
            {
                _isScenario2Selected = value;
                OnPropertyChanged();
                
                // If selecting Scenario 2, deselect Scenario 1
                if (value && _isScenario1Selected)
                {
                    IsScenario1Selected = false;
                }
                
                if (value)
                {
                    LoadScenario2();
                }
            }
        }
    }

    private bool _isDanfossValuesSelected;
    public bool IsDanfossValuesSelected
    {
        get => _isDanfossValuesSelected;
        set
        {
            if (_isDanfossValuesSelected != value)
            {
                _isDanfossValuesSelected = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Replace the SaveMachinesCommand with individual commands for each machine
    private Dictionary<string, ICommand> _saveMachineCommands = new Dictionary<string, ICommand>();
    
    // Current unit to save - mark as nullable
    private ProductionUnit? _currentUnit;
    public ICommand SaveMachineCommand { get; }
    
    public ObservableCollection<ProductionUnit> Machines { get; set; } = new();
    
    private AssetManager _assetManager;
    private List<ProductionUnit> _defaultUnits;

    // Dictionary to store machine type names
    private static readonly Dictionary<string, string> MachineTypes = new()
    {
        { "GB1", "Gas Boiler 1" },
        { "GB2", "Gas Boiler 2" },
        { "OB1", "Oil Boiler 1" },
        { "GM1", "Gas Motor 1" },
        { "HP1", "Heat Pump 1" }
    };

    public static readonly IValueConverter MachineTypeNameConverter = new FuncValueConverter<string, string>(id =>
        id != null && MachineTypes.TryGetValue(id, out var name) ? name : id ?? string.Empty);

    public MachineryViewModel(string userName, bool isDarkTheme) : base(userName, isDarkTheme)
    {
        NavigateToHomeCommand = new Command(() => NavigateToHome());
        NavigateToOptimizerCommand = new Command(() => NavigateToOptimizer());
        NavigateToMachineryCommand = new Command(() => { /* Already on settings page */ });
        NavigateToAboutUsCommand = new Command(() => NavigateToAboutUs());
        SaveMachineCommand = new Command(() => SaveCurrentMachine());
        
        // Initialize scenario commands
        LoadScenario1Command = new Command(() => IsScenario1Selected = !IsScenario1Selected);
        LoadScenario2Command = new Command(() => IsScenario2Selected = !IsScenario2Selected);

        // Initialize command for dismissing the Danfoss info banner
        DismissDanfossInfoCommand = new Command(() => ShowDanfossInfoBanner = false);

        _assetManager = new AssetManager();
        
        // Load default units for scenarios
        _defaultUnits = LoadDefaultUnits();
        
        // Initially load all units without scenario filtering
        var units = _assetManager.GetProductionUnits();

        for (int i = 0; i < units.Count; i++)
        {
            units[i].ImagePath = $"avares://DanfossHeating/Assets/machine{i + 1}.png";
            Console.WriteLine($"Loaded image path: {units[i].ImagePath}");
            
            // Create a separate save command for each machine
            var unit = units[i]; // Capture the unit in a local variable to avoid closure issues
            if (!string.IsNullOrEmpty(unit.Name))
            {
                _saveMachineCommands[unit.Name] = new Command(() => SaveMachine(unit));
            }
        }

        Machines = new ObservableCollection<ProductionUnit>(units);
    
        Console.WriteLine($"MachineryViewModel created for user: {userName}");
    }
    
    public override void SetMainViewModel(MainWindowViewModel mainViewModel)
    {
        base.SetMainViewModel(mainViewModel);
        
        // Initialize from MainViewModel's state
        if (mainViewModel != null)
        {
            IsDanfossValuesSelected = mainViewModel.IsDanfossValuesSelected;
            DisabledMachines = mainViewModel.DisabledMachines;
        }
    }
    
    // Load default units from default_units.json
    private List<ProductionUnit> LoadDefaultUnits()
    {
        try
        {
            string defaultJsonPath = "DanfossHeating/Data/default_units.json";
            string appPath = "Data/default_units.json";
            string jsonPath = "";
            
            // Try to locate the file
            if (File.Exists(defaultJsonPath))
            {
                jsonPath = defaultJsonPath;
            }
            else if (File.Exists(appPath))
            {
                jsonPath = appPath;
            }
            else
            {
                Console.WriteLine("Could not find default_units.json");
                return new List<ProductionUnit>();
            }
            
            // Read and parse the default units
            string json = File.ReadAllText(jsonPath);
            var defaultUnits = JsonSerializer.Deserialize<List<ProductionUnit>>(json) ?? new List<ProductionUnit>();
            
            Console.WriteLine($"Loaded {defaultUnits.Count} default units");
            return defaultUnits;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading default units: {ex.Message}");
            return new List<ProductionUnit>();
        }
    }
    
    // Load Scenario 1: GB1, GB2, OB1 with values from default_units.json
    private void LoadScenario1()
    {
        try
        {
            var currentUnits = _assetManager.GetProductionUnits();
            
            // Create scenario 1 configuration with hard-coded default values
            foreach (var unit in currentUnits)
            {
                if (unit.Name == "GB1")
                {
                    // Default values for GB1
                    unit.MaxHeat = 4.0;
                    unit.MaxElectricity = 0;  // N/A for GB1
                    unit.ProductionCosts = 520;
                    unit.CO2Emissions = 175;
                    unit.FuelConsumption = 0.9;
                }
                else if (unit.Name == "GB2") 
                {
                    // Default values for GB2
                    unit.MaxHeat = 3.0;
                    unit.MaxElectricity = 0;  // N/A for GB2
                    unit.ProductionCosts = 560;
                    unit.CO2Emissions = 130;
                    unit.FuelConsumption = 0.7;
                }
                else if (unit.Name == "OB1")
                {
                    // Default values for OB1
                    unit.MaxHeat = 4.0;
                    unit.MaxElectricity = 0;  // N/A for OB1
                    unit.ProductionCosts = 670;
                    unit.CO2Emissions = 330;
                    unit.FuelConsumption = 1.5;
                }
                else
                {
                    // For units not in Scenario 1, set all values to 0
                    unit.MaxHeat = 0;
                    unit.MaxElectricity = 0;
                    unit.ProductionCosts = 0;
                    unit.CO2Emissions = 0;
                    unit.FuelConsumption = 0;
                }
                
                // Save the updated unit
                _assetManager.SaveProductionUnit(unit);
            }
            
            // Refresh the machines collection
            Machines.Clear();
            foreach (var unit in _assetManager.GetProductionUnits())
            {
                Machines.Add(unit);
            }
            
            Console.WriteLine("Loaded Scenario 1 values");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading Scenario 1: {ex.Message}");
        }
    }
    
    // Load Scenario 2: GB1, OB1, GM1, HP1 (not GB2) with values from default_units.json
    private void LoadScenario2()
    {
        try
        {
            var currentUnits = _assetManager.GetProductionUnits();
            
            // Create scenario 2 configuration with hard-coded default values
            foreach (var unit in currentUnits)
            {
                if (unit.Name == "GB1")
                {
                    // Default values for GB1
                    unit.MaxHeat = 4.0;
                    unit.MaxElectricity = 0;  // N/A for GB1
                    unit.ProductionCosts = 520;
                    unit.CO2Emissions = 175;
                    unit.FuelConsumption = 0.9;
                }
                else if (unit.Name == "OB1")
                {
                    // Default values for OB1
                    unit.MaxHeat = 4.0;
                    unit.MaxElectricity = 0;  // N/A for OB1
                    unit.ProductionCosts = 670;
                    unit.CO2Emissions = 330;
                    unit.FuelConsumption = 1.5;
                }
                else if (unit.Name == "GM1")
                {
                    // Default values for GM1
                    unit.MaxHeat = 3.5;
                    unit.MaxElectricity = 2.6;
                    unit.ProductionCosts = 990;
                    unit.CO2Emissions = 650;
                    unit.FuelConsumption = 1.8;
                }
                else if (unit.Name == "HP1")
                {
                    // Default values for HP1
                    unit.MaxHeat = 6.0;
                    unit.MaxElectricity = -6.0;  // Negative for consumption
                    unit.ProductionCosts = 60;
                    unit.CO2Emissions = 0;  // N/A for HP1
                    unit.FuelConsumption = 0;  // N/A for HP1
                }
                else
                {
                    // For units not in Scenario 2 (GB2), set all values to 0
                    unit.MaxHeat = 0;
                    unit.MaxElectricity = 0;
                    unit.ProductionCosts = 0;
                    unit.CO2Emissions = 0;
                    unit.FuelConsumption = 0;
                }
                
                // Save the updated unit
                _assetManager.SaveProductionUnit(unit);
            }
            
            // Refresh the machines collection
            Machines.Clear();
            foreach (var unit in _assetManager.GetProductionUnits())
            {
                Machines.Add(unit);
            }
            
            Console.WriteLine("Loaded Scenario 2 values");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading Scenario 2: {ex.Message}");
        }
    }
    
    // Method to set current machine before saving
    public void PrepareForSave(ProductionUnit unit)
    {
        _currentUnit = unit;
    }
    
    // Method to save the current unit
    private void SaveCurrentMachine()
    {
        if (_currentUnit != null)
        {
            SaveMachine(_currentUnit);
        }
    }
    
    // Method to get the save command for a specific machine
    public ICommand GetSaveCommand(string machineName)
    {
        if (!string.IsNullOrEmpty(machineName) && _saveMachineCommands.TryGetValue(machineName, out var command))
        {
            return command;
        }
        return new Command(() => { }); // Return a no-op command if not found
    }
    
    public void SaveMachine(ProductionUnit unit)
    {
        try
        {
            // In a real application, this would update the specific machine in the JSON file
            _assetManager.SaveProductionUnit(unit);
            
            // Show a notification or update status
            Console.WriteLine($"Machine {unit.Name} updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving machine data: {ex.Message}");
        }
    }
    
    // Method to disable a machine by setting all values to zero
    public void DisableMachine(ProductionUnit unit)
    {
        try
        {
            // Set all numeric values to zero
            unit.MaxHeat = 0;
            unit.MaxElectricity = 0;
            unit.CO2Emissions = 0;
            unit.ProductionCosts = 0;
            unit.FuelConsumption = 0;
            
            // Add to disabled machines set
            if (!string.IsNullOrEmpty(unit.Name))
            {
                DisabledMachines.Add(unit.Name);
                OnPropertyChanged(nameof(DisabledMachines));
            }
            
            // Save the disabled machine state
            _assetManager.SaveProductionUnit(unit);
            
            // Show a notification or update status
            Console.WriteLine($"Machine {unit.Name} disabled successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disabling machine: {ex.Message}");
        }
    }
    
    // Method to enable a previously disabled machine
    public void EnableMachine(ProductionUnit unit)
    {
        try
        {
            // Remove from disabled machines set
            if (!string.IsNullOrEmpty(unit.Name))
            {
                DisabledMachines.Remove(unit.Name);
                OnPropertyChanged(nameof(DisabledMachines));
            }
            
            // If Danfoss values are selected, load their values, otherwise keep existing values
            if (IsDanfossValuesSelected)
            {
                // Load default values for this unit
                var defaultUnits = LoadDefaultUnits();
                var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == unit.Name);
                
                if (defaultUnit != null)
                {
                    // Apply default values but respect special fields
                    unit.MaxHeat = defaultUnit.MaxHeat;
                    
                    // Special handling for GB and OB units
                    if (unit.Name == "GB1" || unit.Name == "GB2" || unit.Name == "OB1")
                    {
                        unit.MaxElectricity = null; // Keep electricity null for GB and OB units
                    }
                    else
                    {
                        unit.MaxElectricity = defaultUnit.MaxElectricity;
                    }
                    
                    // Special handling for HP unit
                    if (unit.Name == "HP1")
                    {
                        unit.CO2Emissions = null; // Keep CO2 emissions null for HP unit
                        unit.FuelConsumption = null; // Keep fuel consumption null for HP unit
                    }
                    else
                    {
                        unit.CO2Emissions = defaultUnit.CO2Emissions;
                        unit.FuelConsumption = defaultUnit.FuelConsumption;
                    }
                    
                    unit.ProductionCosts = defaultUnit.ProductionCosts;
                }
            }
            // If Danfoss values are not selected, we keep the existing values
            
            // Save the updated unit
            _assetManager.SaveProductionUnit(unit);
            
            Console.WriteLine($"Machine {unit.Name} enabled successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enabling machine: {ex.Message}");
        }
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
