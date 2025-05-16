using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives; // Add this for Popup
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DanfossHeating.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

// Helper extension method for finding controls by type and predicate
public static class ControlExtensions
{
    public static T? FindDescendantOfType<T>(this Control control, Func<T, bool>? predicate = null) where T : Control
    {
        if (control is T typedControl && (predicate == null || predicate(typedControl)))
        {
            return typedControl;
        }

        foreach (var child in control.GetVisualDescendants())
        {
            if (child is T childOfType && (predicate == null || predicate(childOfType)))
            {
                return childOfType;
            }
        }

        return default;
    }
}

namespace DanfossHeating.Views
{
    public partial class MachineryPage : UserControl
    {
        // Property for IsDarkTheme that can be accessed by the UI
        public static readonly StyledProperty<bool> IsDarkThemeProperty =
            AvaloniaProperty.Register<MachineryPage, bool>(nameof(IsDarkTheme));

        public bool IsDarkTheme
        {
            get => GetValue(IsDarkThemeProperty);
            set => SetValue(IsDarkThemeProperty, value);
        }

        private Dictionary<TextBox, Border> _validationBanners = new Dictionary<TextBox, Border>();
        
        // Direct reference to checkboxes for scenario selection
        private CheckBox? _scenario1Checkbox;
        private CheckBox? _scenario2Checkbox;

        private Dictionary<string, IImage> _machineImages = new Dictionary<string, IImage>();

        public MachineryPage()
        {
            InitializeComponent();
            PreloadMachineImages();
            Loaded += MachineryPage_Loaded;
            
            // Subscribe to DataContext changes
            DataContextChanged += MachineryPage_DataContextChanged;
        }

        private void PreloadMachineImages()
        {
            try
            {
                // Preload all machine images
                for (int i = 1; i <= 5; i++)
                {
                    var uri = new Uri($"avares://DanfossHeating/Assets/Machines/machine{i}.png");
                    using var assetStream = AssetLoader.Open(uri);
                    var bitmap = new Bitmap(assetStream);
                    _machineImages[$"machine{i}"] = bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preloading images: {ex.Message}");
            }
        }
        
        private void MachineryPage_DataContextChanged(object? sender, EventArgs e)
        {
            // When DataContext changes, update binding
            UpdateThemeBinding();
        }
        
        private void UpdateThemeBinding()
        {
            if (DataContext is MachineryViewModel vm)
            {
                // Sync the control's IsDarkTheme with the ViewModel's IsDarkTheme
                IsDarkTheme = vm.IsDarkTheme;
                
                // Subscribe to the ViewModel's property changed event
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MachineryViewModel.IsDarkTheme) && sender is MachineryViewModel vm)
            {
                // Update our property when the ViewModel's property changes
                IsDarkTheme = vm.IsDarkTheme;
            }
        }

        private void MachineryPage_Loaded(object? sender, RoutedEventArgs e)
        {
            // Initialize theme state from ViewModel
            UpdateThemeBinding();
            
            // Set up scenario checkboxes
            SetupScenarioCheckboxes();
            
            // Load images and configure fields
            Dispatcher.UIThread.Post(() => LoadMachineImages(), DispatcherPriority.Background);
            Dispatcher.UIThread.Post(() => ConfigureSpecialFields(), DispatcherPriority.Background);
            
            // Apply initial disabled states
            if (DataContext is MachineryViewModel viewModel)
            {
                // Apply disabled states
                foreach (var machine in viewModel.Machines)
                {
                    if (!string.IsNullOrEmpty(machine.Name) && viewModel.DisabledMachines.Contains(machine.Name))
                    {
                        ApplyDisabledState(machine.Name);
                    }
                }

                // Watch for changes to properties
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MachineryViewModel.IsDanfossValuesSelected))
                    {
                        if (viewModel.IsDanfossValuesSelected)
                        {
                            LoadDanfossDefaultValues();
                        }
                    }
                };
            }
        }

        private void ApplyDisabledState(string machineName)
        {
            var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
            if (itemsControl == null) return;

            // Find the card for this machine
            var cards = itemsControl.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                .ToList();

            foreach (var card in cards)
            {
                var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                    tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                
                if (nameTextBlock?.Text == machineName)
                {
                    // Update card opacity
                    card.Opacity = 0.6;

                    // Update image opacity
                    var machineImage = card.FindDescendantOfType<Image>();
                    if (machineImage != null)
                    {
                        machineImage.Opacity = 0.5;
                    }

                    // Update button state
                    var toggleButton = card.FindDescendantOfType<Button>(b => b.Name == "DisableEnableButton");
                    if (toggleButton != null)
                    {
                        var buttonText = toggleButton.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                        if (buttonText != null)
                        {
                            buttonText.Text = "Enable Machine";
                            toggleButton.Background = new SolidColorBrush(Colors.DarkGreen);
                        }
                    }

                    // Show disabled overlay
                    var disabledOverlay = card.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                    if (disabledOverlay != null)
                    {
                        disabledOverlay.IsVisible = true;
                    }

                    // Disable text boxes
                    var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                    foreach (var textBox in textBoxes)
                    {
                        if (textBox.Name == "HeatOutputTextBox" ||
                            textBox.Name == "ElectricityUsageTextBox" ||
                            textBox.Name == "CO2EmissionsTextBox" ||
                            textBox.Name == "ProductionCostsTextBox" ||
                            textBox.Name == "FuelConsumptionTextBox")
                        {
                            textBox.IsEnabled = false;
                            textBox.Text = "0";
                        }
                    }
                    break;
                }
            }
        }

        private void SetupScenarioCheckboxes()
        {
            try
            {
                // Find the scenario checkboxes
                _scenario1Checkbox = this.FindControl<CheckBox>("Scenario1Checkbox");
                _scenario2Checkbox = this.FindControl<CheckBox>("Scenario2Checkbox");
                
                if (_scenario1Checkbox != null)
                {
                    // Add direct event handler for the checkbox
                    _scenario1Checkbox.IsCheckedChanged += Scenario1Checkbox_IsCheckedChanged;
                }
                
                if (_scenario2Checkbox != null)
                {
                    // Add direct event handler for the checkbox
                    _scenario2Checkbox.IsCheckedChanged += Scenario2Checkbox_IsCheckedChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up scenario checkboxes: {ex.Message}");
            }
        }
        
        private void SetupDanfossValuesCheckbox()
        {
            try
            {
                // Find the Danfoss Values checkbox
                var danfossValuesCheckbox = this.FindControl<CheckBox>("DanfossValuesCheckbox");
                
                if (danfossValuesCheckbox != null)
                {
                    // Add direct event handler for the checkbox
                    danfossValuesCheckbox.IsCheckedChanged += DanfossValuesCheckbox_IsCheckedChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up Danfoss Values checkbox: {ex.Message}");
            }
        }
        
        private void DanfossValuesCheckbox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                if (checkbox.IsChecked == true)
                {
                    // Apply Danfoss default values
                    LoadDanfossDefaultValues();
                }
                // Remove the else clause that was calling ClearAllValues()
            }
        }
        
        private void Scenario1Checkbox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                // If Scenario 1 is checked, uncheck Scenario 2
                if (checkbox.IsChecked == true)
                {
                    if (_scenario2Checkbox != null && _scenario2Checkbox.IsChecked == true)
                    {
                        _scenario2Checkbox.IsChecked = false;
                    }
                    
                    // Apply Scenario 1 values
                    LoadScenario1Values();
                }
                else if (_scenario2Checkbox != null && _scenario2Checkbox.IsChecked != true)
                {
                    // If both checkboxes are unchecked, restore original values
                    RestoreOriginalValues();
                }
            }
        }
        
        private void Scenario2Checkbox_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                // If Scenario 2 is checked, uncheck Scenario 1
                if (checkbox.IsChecked == true)
                {
                    if (_scenario1Checkbox != null && _scenario1Checkbox.IsChecked == true)
                    {
                        _scenario1Checkbox.IsChecked = false;
                    }
                    
                    // Apply Scenario 2 values
                    LoadScenario2Values();
                }
                else if (_scenario1Checkbox != null && _scenario1Checkbox.IsChecked != true)
                {
                    // If both checkboxes are unchecked, restore original values
                    RestoreOriginalValues();
                }
            }
        }
        
        // Restore original values from production_units.json when both scenarios are unchecked
        private void RestoreOriginalValues()
        {
            if (DataContext is MachineryViewModel viewModel)
            {
                // Create a new instance of AssetManager to reload the original values
                var assetManager = new AssetManager();
                var units = assetManager.GetProductionUnits();
                
                // Get all machine cards
                var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
                if (itemsControl == null) return;
                
                // Find all cards in the ItemsControl
                var cards = itemsControl.GetVisualDescendants()
                    .OfType<Border>()
                    .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                    .ToList();
                
                // Update UI for each card
                foreach (var card in cards)
                {
                    // Find the machine name in this card
                    var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                        tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                    
                    if (nameTextBlock == null) continue;
                    
                    string machineName = nameTextBlock.Text ?? string.Empty;
                    var originalUnit = units.FirstOrDefault(u => u.Name == machineName);
                    
                    if (originalUnit == null) continue;
                    
                    // Find all the text boxes for this machine
                    var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                    
                    foreach (var textBox in textBoxes)
                    {
                        if (textBox.Name == "HeatOutputTextBox")
                            textBox.Text = originalUnit.MaxHeat?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (textBox.Name == "ElectricityUsageTextBox")
                            textBox.Text = originalUnit.MaxElectricity?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (textBox.Name == "CO2EmissionsTextBox") 
                            textBox.Text = originalUnit.CO2Emissions?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (textBox.Name == "ProductionCostsTextBox")
                            textBox.Text = originalUnit.ProductionCosts?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (textBox.Name == "FuelConsumptionTextBox")
                            textBox.Text = originalUnit.FuelConsumption?.ToString(CultureInfo.InvariantCulture) ?? "0";
                    }
                    
                    // Enable all machines
                    var disabledOverlay = card.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                    if (disabledOverlay != null)
                    {
                        disabledOverlay.IsVisible = false;
                    }
                    
                    // Restore card opacity
                    card.Opacity = 1.0;
                    
                    // Restore machine image opacity
                    var machineImage = card.FindDescendantOfType<Image>();
                    if (machineImage != null)
                    {
                        machineImage.Opacity = 1.0;
                    }
                    
                    // Update the enable/disable button
                    var toggleButton = card.FindDescendantOfType<Button>(b => b.Name == "DisableEnableButton");
                    if (toggleButton != null)
                    {
                        var buttonText = toggleButton.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                        if (buttonText != null)
                        {
                            buttonText.Text = "Disable Machine";
                            toggleButton.Background = new SolidColorBrush(Color.Parse("#707070"));
                        }
                    }
                }
                
                // Save the changes through the view model
                foreach (var unit in units)
                {
                    var machine = viewModel.Machines.FirstOrDefault(m => m.Name == unit.Name);
                    if (machine != null)
                    {
                        machine.MaxHeat = unit.MaxHeat;
                        machine.MaxElectricity = unit.MaxElectricity;
                        machine.CO2Emissions = unit.CO2Emissions;
                        machine.ProductionCosts = unit.ProductionCosts;
                        machine.FuelConsumption = unit.FuelConsumption;
                        
                        viewModel.SaveMachine(machine);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Restored original values from production_units.json");
            }
        }
        
        private List<ProductionUnit> LoadDefaultUnits()
        {
            try
            {
                // Try to load from typical locations
                string[] possiblePaths = {
                    "Data/default_units.json",
                    "DanfossHeating/Data/default_units.json",
                    "../../Data/default_units.json",
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/default_units.json")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var units = JsonSerializer.Deserialize<List<ProductionUnit>>(json);
                        if (units != null && units.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Successfully loaded default_units.json from {path}");
                            return units;
                        }
                    }
                }
                
                // If file not found, create hard-coded default values
                System.Diagnostics.Debug.WriteLine("Default units file not found, using hard-coded values");
                return new List<ProductionUnit>
                {
                    new ProductionUnit { Name = "GB1", MaxHeat = 4.0, MaxElectricity = 0, ProductionCosts = 520, CO2Emissions = 175, FuelConsumption = 0.9 },
                    new ProductionUnit { Name = "GB2", MaxHeat = 3.0, MaxElectricity = 0, ProductionCosts = 560, CO2Emissions = 130, FuelConsumption = 0.7 },
                    new ProductionUnit { Name = "OB1", MaxHeat = 4.0, MaxElectricity = 0, ProductionCosts = 670, CO2Emissions = 330, FuelConsumption = 1.5 },
                    new ProductionUnit { Name = "GM1", MaxHeat = 3.5, MaxElectricity = 2.6, ProductionCosts = 990, CO2Emissions = 650, FuelConsumption = 1.8 },
                    new ProductionUnit { Name = "HP1", MaxHeat = 6.0, MaxElectricity = -6.0, ProductionCosts = 60, CO2Emissions = 0, FuelConsumption = 0 }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading default units: {ex.Message}");
                return new List<ProductionUnit>();
            }
        }
        
        private void LoadScenario1Values()
        {
            var defaultUnits = LoadDefaultUnits();
            if (defaultUnits.Count == 0) return;
            
            // Get all machine cards
            var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
            if (itemsControl == null) return;
            
            // Find all cards in the ItemsControl
            var cards = itemsControl.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                .ToList();
            
            foreach (var card in cards)
            {
                // Find the machine name in this card
                var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                    tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                
                if (nameTextBlock == null) continue;
                
                string machineName = nameTextBlock.Text ?? string.Empty;
                var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == machineName);
                
                if (defaultUnit == null) continue;
                
                // For scenario 1, we want GB1, GB2, and OB1 active, others disabled
                bool isActive = machineName == "GB1" || machineName == "GB2" || machineName == "OB1";
                
                // Find all the text boxes for this machine
                var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                
                // Check which fields should remain empty (disabled fields)
                bool isGB = machineName == "GB1" || machineName == "GB2";
                bool isOB = machineName == "OB1";
                bool isHP = machineName == "HP1";
                
                foreach (var textBox in textBoxes)
                {
                    // Always keep disabled fields empty, regardless of scenario
                    if ((isGB || isOB) && textBox.Name == "ElectricityUsageTextBox") {
                        textBox.Text = ""; // Keep empty for GB and OB units
                    }
                    else if (isHP && (textBox.Name == "CO2EmissionsTextBox" || textBox.Name == "FuelConsumptionTextBox")) {
                        textBox.Text = ""; // Keep empty for HP unit
                    }
                    // For other fields, use the scenario logic
                    else if (textBox.Name == "HeatOutputTextBox")
                    {
                        if (isActive)
                            textBox.Text = defaultUnit.MaxHeat?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "ElectricityUsageTextBox")
                    {
                        if (isActive && !(isGB || isOB))
                            textBox.Text = defaultUnit.MaxElectricity?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "CO2EmissionsTextBox")
                    {
                        if (isActive && !isHP)
                            textBox.Text = defaultUnit.CO2Emissions?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "ProductionCostsTextBox")
                    {
                        if (isActive)
                            textBox.Text = defaultUnit.ProductionCosts?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "FuelConsumptionTextBox")
                    {
                        if (isActive && !isHP)
                            textBox.Text = defaultUnit.FuelConsumption?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                }
                
                // Find and update the disabled overlay
                var disabledOverlay = card.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                if (disabledOverlay != null)
                {
                    disabledOverlay.IsVisible = !isActive;
                }
                
                // Update card opacity
                card.Opacity = isActive ? 1.0 : 0.6;
                
                // Find the machine image and update its opacity
                var machineImage = card.FindDescendantOfType<Image>();
                if (machineImage != null)
                {
                    machineImage.Opacity = isActive ? 1.0 : 0.5;
                }
                
                // Update the enable/disable button
                var toggleButton = card.FindDescendantOfType<Button>(b => b.Name == "DisableEnableButton");
                if (toggleButton != null)
                {
                    var buttonText = toggleButton.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                    if (buttonText != null)
                    {
                        buttonText.Text = isActive ? "Disable Machine" : "Enable Machine";
                        toggleButton.Background = new SolidColorBrush(isActive ? 
                            Color.Parse("#707070") : Color.Parse("#008000"));
                    }
                }
            }
            
            // Trigger save for each machine to persist changes
            if (DataContext is MachineryViewModel viewModel)
            {
                foreach (var machine in viewModel.Machines)
                {
                    bool isActive = machine.Name == "GB1" || machine.Name == "GB2" || machine.Name == "OB1";
                    
                    if (isActive)
                    {
                        var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == machine.Name);
                        if (defaultUnit != null)
                        {
                            machine.MaxHeat = defaultUnit.MaxHeat;
                            
                            // Special handling for GB and OB units electricity
                            if (machine.Name == "GB1" || machine.Name == "GB2" || machine.Name == "OB1") {
                                machine.MaxElectricity = null; // Keep electricity null for GB and OB units
                            } else {
                                machine.MaxElectricity = defaultUnit.MaxElectricity;
                            }
                            
                            // Special handling for HP unit
                            if (machine.Name == "HP1") {
                                machine.CO2Emissions = null; // Keep CO2 emissions null for HP unit
                                machine.FuelConsumption = null; // Keep fuel consumption null for HP unit
                            } else {
                                machine.CO2Emissions = defaultUnit.CO2Emissions;
                                machine.FuelConsumption = defaultUnit.FuelConsumption;
                            }
                            
                            machine.ProductionCosts = defaultUnit.ProductionCosts;
                        }
                    }
                    else
                    {
                        machine.MaxHeat = 0;
                        machine.MaxElectricity = 0;
                        machine.CO2Emissions = 0;
                        machine.ProductionCosts = 0;
                        machine.FuelConsumption = 0;
                    }
                    
                    viewModel.SaveMachine(machine);
                }
            }
            
            // Make sure special fields stay properly configured
            ConfigureSpecialFields();
            
            System.Diagnostics.Debug.WriteLine("Applied Scenario 1 values");
        }
        
        private void LoadScenario2Values()
        {
            var defaultUnits = LoadDefaultUnits();
            if (defaultUnits.Count == 0) return;
            
            // Get all machine cards
            var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
            if (itemsControl == null) return;
            
            // Find all cards in the ItemsControl
            var cards = itemsControl.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                .ToList();
            
            foreach (var card in cards)
            {
                // Find the machine name in this card
                var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                    tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                
                if (nameTextBlock == null) continue;
                
                string machineName = nameTextBlock.Text ?? string.Empty;
                var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == machineName);
                
                if (defaultUnit == null) continue;
                
                // For scenario 2, we want GB1, OB1, GM1, and HP1 active, not GB2
                bool isActive = machineName == "GB1" || machineName == "OB1" || 
                               machineName == "GM1" || machineName == "HP1";
                
                // Find all the text boxes for this machine
                var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                
                // Check which fields should remain empty (disabled fields)
                bool isGB = machineName == "GB1" || machineName == "GB2";
                bool isOB = machineName == "OB1";
                bool isHP = machineName == "HP1";
                
                foreach (var textBox in textBoxes)
                {
                    // Always keep disabled fields empty, regardless of scenario
                    if ((isGB || isOB) && textBox.Name == "ElectricityUsageTextBox") {
                        textBox.Text = ""; // Keep empty for GB and OB units
                    }
                    else if (isHP && (textBox.Name == "CO2EmissionsTextBox" || textBox.Name == "FuelConsumptionTextBox")) {
                        textBox.Text = ""; // Keep empty for HP unit
                    }
                    // For other fields, use the scenario logic
                    else if (textBox.Name == "HeatOutputTextBox")
                    {
                        if (isActive)
                            textBox.Text = defaultUnit.MaxHeat?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "ElectricityUsageTextBox")
                    {
                        if (isActive && !(isGB || isOB))
                            textBox.Text = defaultUnit.MaxElectricity?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "CO2EmissionsTextBox")
                    {
                        if (isActive && !isHP)
                            textBox.Text = defaultUnit.CO2Emissions?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "ProductionCostsTextBox")
                    {
                        if (isActive)
                            textBox.Text = defaultUnit.ProductionCosts?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else
                            textBox.Text = "0";
                    }
                    else if (textBox.Name == "FuelConsumptionTextBox")
                    {
                        if (isActive && !isHP)
                            textBox.Text = defaultUnit.FuelConsumption?.ToString(CultureInfo.InvariantCulture) ?? "0";
                        else if (!isActive)
                            textBox.Text = "0";
                    }
                }
                
                // Find and update the disabled overlay
                var disabledOverlay = card.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                if (disabledOverlay != null)
                {
                    disabledOverlay.IsVisible = !isActive;
                }
                
                // Update card opacity
                card.Opacity = isActive ? 1.0 : 0.6;
                
                // Find the machine image and update its opacity
                var machineImage = card.FindDescendantOfType<Image>();
                if (machineImage != null)
                {
                    machineImage.Opacity = isActive ? 1.0 : 0.5;
                }
                
                // Update the enable/disable button
                var toggleButton = card.FindDescendantOfType<Button>(b => b.Name == "DisableEnableButton");
                if (toggleButton != null)
                {
                    var buttonText = toggleButton.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                    if (buttonText != null)
                    {
                        buttonText.Text = isActive ? "Disable Machine" : "Enable Machine";
                        toggleButton.Background = new SolidColorBrush(isActive ? 
                            Color.Parse("#707070") : Color.Parse("#008000"));
                    }
                }
            }
            
            // Trigger save for each machine to persist changes
            if (DataContext is MachineryViewModel viewModel)
            {
                foreach (var machine in viewModel.Machines)
                {
                    bool isActive = machine.Name == "GB1" || machine.Name == "OB1" || 
                                    machine.Name == "GM1" || machine.Name == "HP1";
                    
                    if (isActive)
                    {
                        var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == machine.Name);
                        if (defaultUnit != null)
                        {
                            machine.MaxHeat = defaultUnit.MaxHeat;
                            
                            // Special handling for GB and OB units electricity
                            if (machine.Name == "GB1" || machine.Name == "GB2" || machine.Name == "OB1") {
                                machine.MaxElectricity = null; // Keep electricity null for GB and OB units
                            } else {
                                machine.MaxElectricity = defaultUnit.MaxElectricity;
                            }
                            
                            // Special handling for HP unit
                            if (machine.Name == "HP1") {
                                machine.CO2Emissions = null; // Keep CO2 emissions null for HP unit
                                machine.FuelConsumption = null; // Keep fuel consumption null for HP unit
                            } else {
                                machine.CO2Emissions = defaultUnit.CO2Emissions;
                                machine.FuelConsumption = defaultUnit.FuelConsumption;
                            }
                            
                            machine.ProductionCosts = defaultUnit.ProductionCosts;
                        }
                    }
                    else
                    {
                        machine.MaxHeat = 0;
                        machine.MaxElectricity = 0;
                        machine.CO2Emissions = 0;
                        machine.ProductionCosts = 0;
                        machine.FuelConsumption = 0;
                    }
                    
                    viewModel.SaveMachine(machine);
                }
            }
            
            // Make sure special fields stay properly configured
            ConfigureSpecialFields();
            
            System.Diagnostics.Debug.WriteLine("Applied Scenario 2 values");
        }
        
        // Method to configure special fields for specific units
        private void ConfigureSpecialFields()
        {
            try
            {
                var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
                if (itemsControl == null) return;
                
                // Find all cards in the ItemsControl
                var cards = itemsControl.GetVisualDescendants()
                    .OfType<Border>()
                    .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                    .ToList();
                
                foreach (var card in cards)
                {
                    // Find the machine name in this card
                    var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                        tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                    
                    if (nameTextBlock != null)
                    {
                        string machineName = nameTextBlock.Text ?? string.Empty; // Use null-coalescing operator to avoid null-to-non-nullable conversion
                        
                        // GB1, GB2: Electricity Usage N/A
                        if (machineName == "GB1" || machineName == "GB2")
                        {
                            ConfigureFieldAsNA(card, "ElectricityUsageTextBox", "Electricity Usage (N/A)");
                            
                            // Ensure the model value is also null/empty
                            if (DataContext is MachineryViewModel viewModel)
                            {
                                var machine = viewModel.Machines.FirstOrDefault(m => m.Name == machineName);
                                if (machine != null)
                                {
                                    machine.MaxElectricity = null;
                                }
                            }
                        }
                        // OB1: Electricity Usage N/A
                        else if (machineName == "OB1")
                        {
                            ConfigureFieldAsNA(card, "ElectricityUsageTextBox", "Electricity Usage (N/A)");
                            
                            // Ensure the model value is also null/empty
                            if (DataContext is MachineryViewModel viewModel)
                            {
                                var machine = viewModel.Machines.FirstOrDefault(m => m.Name == machineName);
                                if (machine != null)
                                {
                                    machine.MaxElectricity = null;
                                }
                            }
                        }
                        // HP1: CO2 Emissions and Fuel Consumption N/A
                        else if (machineName == "HP1")
                        {
                            ConfigureFieldAsNA(card, "CO2EmissionsTextBox", "CO₂ Emissions (N/A)");
                            ConfigureFieldAsNA(card, "FuelConsumptionTextBox", "Fuel Consumption (N/A)");
                            
                            // Ensure the model values are also null/empty
                            if (DataContext is MachineryViewModel viewModel)
                            {
                                var machine = viewModel.Machines.FirstOrDefault(m => m.Name == machineName);
                                if (machine != null)
                                {
                                    machine.CO2Emissions = null;
                                    machine.FuelConsumption = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ConfigureSpecialFields: {ex.Message}");
            }
        }
        
        // Helper method to configure a field as N/A
        private void ConfigureFieldAsNA(Border card, string textBoxName, string labelText)
        {
            var textBox = card.FindDescendantOfType<TextBox>(tb => tb.Name == textBoxName);
            
            if (textBox != null)
            {
                // Find the label for this textbox
                if (textBox.Parent is Grid grid)
                {
                    int row = Grid.GetRow(textBox);
                    var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault(tb => 
                        Grid.GetRow(tb) == row);
                        
                    if (textBlock != null)
                    {
                        // Grey out the label
                        textBlock.Foreground = new SolidColorBrush(Color.Parse("#999999"));
                        textBlock.Text = labelText;
                    }
                    
                    // Disable and style the textbox
                    textBox.IsEnabled = false;
                    textBox.Opacity = 0.7;
                    textBox.Text = ""; // Clear any existing text
                    textBox.Background = new SolidColorBrush(Color.Parse("#EEEEEE"));
                    textBox.Foreground = new SolidColorBrush(Color.Parse("#999999"));
                    
                    // Explicitly set the DataContext's property to null if possible
                    if (textBox.DataContext is ProductionUnit unit)
                    {
                        // Determine which property to set to null based on the textbox name
                        if (textBoxName == "ElectricityUsageTextBox")
                            unit.MaxElectricity = null;
                        else if (textBoxName == "CO2EmissionsTextBox")
                            unit.CO2Emissions = null;
                        else if (textBoxName == "FuelConsumptionTextBox")
                            unit.FuelConsumption = null;
                    }
                    
                    // Find associated validation banner and hide it
                    var banner = FindValidationBanner(textBox);
                    if (banner != null)
                    {
                        banner.IsVisible = false;
                    }
                }
            }
        }

        // Update the LoadMachineImages method to use preloaded images
        private void LoadMachineImages()
        {
            try
            {
                var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
                if (itemsControl == null)
                    return;

                // Find all Image controls within Borders having the "ImageBorder" class
                var images = itemsControl.GetVisualDescendants()
                    .OfType<Image>()
                    .Where(img => img.GetVisualParent() is Grid grid &&
                                  grid.GetVisualParent() is Border border &&
                                  border.Classes.Contains("ImageBorder"))
                    .ToList();

                // Set different images for each found Image control
                for (int j = 0; j < images.Count; j++)
                {
                    int imageIndex = (j % 5) + 1; // Cycle through machine1.png to machine5.png
                    string imageKey = $"machine{imageIndex}";
                    
                    if (_machineImages.TryGetValue(imageKey, out var bitmap))
                    {
                        images[j].Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadMachineImages: {ex.Message}");
            }
        }

        // Add the missing SaveButton_Click handler
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement logic to save changes for the specific machine
            // Example:
            if (sender is Button button && button.Tag is ProductionUnit unit &&
                DataContext is MachineryViewModel viewModel)
            {
                // Find the parent container (Grid) of the button
                Grid? parentGrid = button.FindAncestorOfType<Grid>();
                if (parentGrid == null) return;

                // Find all textboxes within the same card
                var textBoxes = parentGrid.GetVisualDescendants().OfType<TextBox>().ToList();
                bool hasValidationError = false;

                // Optional: Re-validate all fields before saving
                foreach (var textBox in textBoxes)
                {
                    // Trigger LostFocus logic to validate and potentially show banners
                    NumericTextBox_LostFocus(textBox, new RoutedEventArgs());
                    var banner = FindValidationBanner(textBox);
                    if (banner != null && banner.IsVisible)
                    {
                        hasValidationError = true;
                    }
                }

                if (!hasValidationError)
                {
                    // If validation passes, proceed with saving
                    // The bindings should have updated the 'unit' object already.
                    viewModel.SaveMachine(unit); 
                    System.Diagnostics.Debug.WriteLine($"Save clicked for unit: {unit.Name}");

                    // Optional: Provide user feedback (e.g., temporary message)
                }
                else
                {
                    // Optional: Notify user that validation errors prevent saving
                    System.Diagnostics.Debug.WriteLine($"Save prevented due to validation errors for unit: {unit.Name}");
                }
            }
        }

        private void ToggleEnableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductionUnit unit &&
                DataContext is MachineryViewModel viewModel)
            {
                // Find the TextBlock inside the button
                var buttonText = button.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                if (buttonText == null) return;

                // Find the parent container (Grid) of the button
                Grid? parentGrid = button.FindAncestorOfType<Grid>();
                if (parentGrid == null) return;

                // Find the ancestor Border which is the main card
                Border? cardBorder = null;
                Visual? current = parentGrid;
                int maxLevels = 10;
                
                while (current != null && maxLevels-- > 0)
                {
                    if (current is Border b && b.Width == 280 && b.Name != "ValidationBanner")
                    {
                        cardBorder = b;
                        break;
                    }
                    current = current.GetVisualParent();
                }

                if (cardBorder == null) return;

                // Find required UI elements
                Image? machineImage = cardBorder.FindDescendantOfType<Image>();
                TextBlock? disabledOverlay = cardBorder.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                var textBoxes = parentGrid.GetVisualDescendants().OfType<TextBox>().ToList();

                // Determine machine type
                bool isGB = unit.Name == "GB1" || unit.Name == "GB2";
                bool isOB = unit.Name == "OB1";
                bool isHP = unit.Name == "HP1";

                if (buttonText.Text == "Disable Machine")
                {
                    // DISABLE MACHINE
                    viewModel.DisableMachine(unit);

                    // Update Button
                    buttonText.Text = "Enable Machine";
                    button.Background = new SolidColorBrush(Colors.DarkGreen);

                    // Update visuals
                    cardBorder.Opacity = 0.6;
                    if (machineImage != null) machineImage.Opacity = 0.5;
                    if (disabledOverlay != null) disabledOverlay.IsVisible = true;

                    // Disable textboxes and set values to 0
                    foreach (var textBox in textBoxes)
                    {
                        textBox.IsEnabled = false;
                        
                        bool isPermanentlyDisabled = (isGB || isOB) && textBox.Name == "ElectricityUsageTextBox" ||
                                                   isHP && (textBox.Name == "CO2EmissionsTextBox" || textBox.Name == "FuelConsumptionTextBox");

                        if (!isPermanentlyDisabled)
                        {
                            textBox.Text = "0";
                        }
                    }
                }
                else
                {
                    // ENABLE MACHINE
                    viewModel.EnableMachine(unit);

                    // Update Button
                    buttonText.Text = "Disable Machine";
                    button.Background = new SolidColorBrush(Color.Parse("#707070"));

                    // Update visuals
                    cardBorder.Opacity = 1.0;
                    if (machineImage != null) machineImage.Opacity = 1.0;
                    if (disabledOverlay != null) disabledOverlay.IsVisible = false;

                    // Re-enable textboxes and update values
                    foreach (var textBox in textBoxes)
                    {
                        if ((isGB || isOB) && textBox.Name == "ElectricityUsageTextBox")
                        {
                            ConfigureFieldAsNA(cardBorder, "ElectricityUsageTextBox", "Electricity Usage (N/A)");
                        }
                        else if (isHP && textBox.Name == "CO2EmissionsTextBox")
                        {
                            ConfigureFieldAsNA(cardBorder, "CO2EmissionsTextBox", "CO₂ Emissions (N/A)");
                        }
                        else if (isHP && textBox.Name == "FuelConsumptionTextBox")
                        {
                            ConfigureFieldAsNA(cardBorder, "FuelConsumptionTextBox", "Fuel Consumption (N/A)");
                        }
                        else
                        {
                            textBox.IsEnabled = true;
                            
                            // If Danfoss values are selected, update with default values
                            if (viewModel.IsDanfossValuesSelected)
                            {
                                var defaultUnits = LoadDefaultUnits();
                                var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == unit.Name);
                                
                                if (defaultUnit != null)
                                {
                                    switch (textBox.Name)
                                    {
                                        case "HeatOutputTextBox":
                                            textBox.Text = defaultUnit.MaxHeat?.ToString(CultureInfo.InvariantCulture) ?? "0";
                                            break;
                                        case "ElectricityUsageTextBox":
                                            textBox.Text = defaultUnit.MaxElectricity?.ToString(CultureInfo.InvariantCulture) ?? "0";
                                            break;
                                        case "CO2EmissionsTextBox":
                                            textBox.Text = defaultUnit.CO2Emissions?.ToString(CultureInfo.InvariantCulture) ?? "0";
                                            break;
                                        case "ProductionCostsTextBox":
                                            textBox.Text = defaultUnit.ProductionCosts?.ToString(CultureInfo.InvariantCulture) ?? "0";
                                            break;
                                        case "FuelConsumptionTextBox":
                                            textBox.Text = defaultUnit.FuelConsumption?.ToString(CultureInfo.InvariantCulture) ?? "0";
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Enhance the TextInput handler
        private void NumericTextBox_TextInput(object sender, TextInputEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(e.Text))
            {
                var banner = FindValidationBanner(textBox); // Find associated banner

                bool isNonNumeric = false;
                foreach (char c in e.Text)
                {
                    if (!char.IsDigit(c) && c != '.') { isNonNumeric = true; break; }
                    if (c == '.' && (textBox.Text?.Contains('.') ?? false)) { isNonNumeric = true; break; }
                }

                if (isNonNumeric)
                {
                    e.Handled = true;
                    if (banner != null)
                    {
                        string fieldName = GetFieldName(textBox);
                        // Find the specific TextBlock within the banner
                        var messageBlock = banner.FindDescendantOfType<TextBlock>(tb => tb.Name == "ValidationMessage");
                        if (messageBlock != null)
                        {
                            messageBlock.Text = $"Invalid character for {fieldName}";
                            banner.IsVisible = true;
                        }
                    }
                }
                else
                {
                    string potentialText = (textBox.Text ?? "") + e.Text;
                    if (double.TryParse(potentialText, NumberStyles.Any, CultureInfo.InvariantCulture, out _) || potentialText == "." || string.IsNullOrEmpty(potentialText))
                    {
                        if (banner != null)
                        {
                            banner.IsVisible = false;
                        }
                    }
                }
            }
        }

        // Refined LostFocus handler with conditional validation
        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ProductionUnit unit) // Ensure DataContext is a ProductionUnit
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                var banner = FindValidationBanner(textBox);
                if (banner == null) return;

                if (!_validationBanners.ContainsKey(textBox)) { _validationBanners[textBox] = banner; }

                string fieldName = GetFieldName(textBox); // Gets the label text like "Electricity Usage"
                string textBoxName = textBox.Name ?? string.Empty; // Gets the TextBox name like "ElectricityUsageTextBox"

                var messageBlock = banner.FindDescendantOfType<TextBlock>(tb => tb.Name == "ValidationMessage");
                bool isValidNumber = double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                bool isEmpty = string.IsNullOrWhiteSpace(text);

                bool allowEmpty = false;

                // --- Apply Specific Rules ---
                string unitTypePrefix = unit.Name?.Length >= 2 ? unit.Name.Substring(0, 2) : string.Empty;

                if (unitTypePrefix == "GB" && textBoxName == "ElectricityUsageTextBox")
                {
                    allowEmpty = true;
                    
                    // Grey out the electricity usage field for GB units (both GB1 and GB2)
                    if (unit.Name == "GB1" || unit.Name == "GB2")
                    {
                        // Find the label and make it grey
                        if (textBox.Parent is Grid grid)
                        {
                            int row = Grid.GetRow(textBox);
                            // Find TextBlock in same row (should be the label)
                            var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == row);
                            if (textBlock != null)
                            {
                                // Apply grey color to indicate it's not applicable
                                textBlock.Foreground = new SolidColorBrush(Color.Parse("#999999"));
                                textBlock.Text = "Electricity Usage (N/A)";
                            }

                            // Disable the textbox and style it as inactive
                            textBox.IsEnabled = false;
                            textBox.Opacity = 0.7;
                            textBox.Text = ""; // Clear any existing text
                            textBox.Background = new SolidColorBrush(Color.Parse("#EEEEEE"));
                            textBox.Foreground = new SolidColorBrush(Color.Parse("#999999"));
                            
                            // Cancel validation for this field
                            banner.IsVisible = false;
                            return;
                        }
                    }
                }
                else if (unitTypePrefix == "HP" && (textBoxName == "CO2EmissionsTextBox" || textBoxName == "FuelConsumptionTextBox"))
                {
                    allowEmpty = true;
                }
                else if (unitTypePrefix == "OB" && textBoxName == "ElectricityUsageTextBox") // Added rule for OB units
                {
                    allowEmpty = true;
                }
                else if (unitTypePrefix == "GM")
                {
                    // All fields mandatory for GM1 (default behavior)
                    allowEmpty = false;
                }
                // Add more rules here if needed for other unit types or fields

                // --- Determine Action ---
                if (isValidNumber && !isEmpty)
                {
                    // Valid number entered
                    banner.IsVisible = false;
                }
                else if (isEmpty)
                {
                    if (allowEmpty)
                    {
                        // Field is optional and empty, ensure it's truly empty for binding if needed
                        banner.IsVisible = false;
                    }
                    else // Mandatory field is empty
                    {
                        textBox.Text = "0"; // Default to 0
                        if (messageBlock != null)
                        {
                            messageBlock.Text = $"{fieldName} is mandatory.";
                            banner.IsVisible = true;
                        }
                    }
                }
                else // Not a valid number and not empty
                {
                    textBox.Text = "0"; // Default invalid input to 0
                    if (messageBlock != null)
                    {
                        messageBlock.Text = $"Invalid input for {fieldName}.";
                        banner.IsVisible = true;
                    }
                }
            }
        }

        // Helper to get the field name associated with a TextBox
        private string GetFieldName(TextBox textBox)
        {
            string fieldName = "this field";
            // Try finding the TextBlock label within the same parent Grid row
            if (textBox.Parent is Grid grid)
            {
                 int row = Grid.GetRow(textBox);
                 // Find the first TextBlock in the same row. Assume it's the label.
                 var textBlock = grid.Children.OfType<TextBlock>()
                                     .FirstOrDefault(tb => Grid.GetRow(tb) == row);
                 if (textBlock != null)
                 {
                     fieldName = textBlock.Text ?? fieldName;
                 }
            }
            // Fallback using TextBox name (less reliable for dynamic content)
            else if (!string.IsNullOrEmpty(textBox.Name))
            {
                 // Simple fallback: remove "TextBox" suffix
                 fieldName = textBox.Name.Replace("TextBox", "");
                 // Optional: Add spaces before capital letters for better readability
                 fieldName = Regex.Replace(fieldName, "([A-Z])", " $1").Trim();
            }
            return fieldName;
        }

        // Updated FindValidationBanner method - More targeted search
        private Border? FindValidationBanner(Control control)
        {
            // Search upwards for the root container of the DataTemplate instance.
            // This is often a ContentPresenter or the direct root element of the template.
            Control? parent = control.Parent as Control; // Start with logical parent
            int maxLevels = 10; // Limit search depth

            while (parent != null && maxLevels-- > 0)
            {
                // Check if the parent is the ContentPresenter for the item
                if (parent is ContentPresenter contentPresenter)
                {
                    // Search within the ContentPresenter's visual children for the banner
                    return contentPresenter.FindDescendantOfType<Border>(b => b.Name == "ValidationBanner");
                }
                // Check if the parent is the root Border of our card template
                if (parent is Border cardBorder && cardBorder.Width == 280 && cardBorder.Name != "ValidationBanner")
                {
                    // Search down from this assumed root border
                    return cardBorder.FindDescendantOfType<Border>(b => b.Name == "ValidationBanner");
                }

                // Move up the logical tree
                parent = parent.Parent as Control;
            }

            // Fallback: If logical tree search fails, try searching visual tree upwards
            Visual? visualAncestor = control; // Start from the control itself
            maxLevels = 10;
            while (visualAncestor != null && maxLevels-- > 0)
            {
                 // Check if the ancestor is the ContentPresenter
                 if (visualAncestor is ContentPresenter cp)
                 {
                     return cp.FindDescendantOfType<Border>(b => b.Name == "ValidationBanner");
                 }
                 // Check if the ancestor is the root Border of the card
                 if (visualAncestor is Border cardBorder && cardBorder.Width == 280 && cardBorder.Name != "ValidationBanner")
                 {
                     return cardBorder.FindDescendantOfType<Border>(b => b.Name == "ValidationBanner");
                 }
                 visualAncestor = visualAncestor.GetVisualParent(); // Use GetVisualParent()
            }

            return null; // Banner not found
        }

        // Load default values from default_units.json for all machines
        private void LoadDanfossDefaultValues()
        {
            var defaultUnits = LoadDefaultUnits();
            if (defaultUnits.Count == 0) return;
            
            var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
            if (itemsControl == null) return;
            
            var cards = itemsControl.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                .ToList();

            // Get the view model and disabled machines state
            var viewModel = DataContext as MachineryViewModel;
            if (viewModel == null) return;
            
            foreach (var card in cards)
            {
                var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                    tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                
                if (nameTextBlock == null) continue;
                
                string machineName = nameTextBlock.Text ?? string.Empty;
                var defaultUnit = defaultUnits.FirstOrDefault(u => u.Name == machineName);
                
                if (defaultUnit == null) continue;
                
                bool isDisabled = viewModel.DisabledMachines.Contains(machineName);
                
                // Check for special fields that should remain empty
                bool isGB = machineName == "GB1" || machineName == "GB2";
                bool isOB = machineName == "OB1";
                bool isHP = machineName == "HP1";
                
                var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                
                // Handle the machine model values first
                var machine = viewModel.Machines.FirstOrDefault(m => m.Name == machineName);
                if (machine != null)
                {
                    if (isDisabled)
                    {
                        machine.MaxHeat = 0;
                        machine.MaxElectricity = isGB || isOB ? null : 0;
                        machine.CO2Emissions = isHP ? null : 0;
                        machine.FuelConsumption = isHP ? null : 0;
                        machine.ProductionCosts = 0;
                    }
                    else
                    {
                        machine.MaxHeat = defaultUnit.MaxHeat;
                        machine.MaxElectricity = (isGB || isOB) ? null : defaultUnit.MaxElectricity;
                        machine.CO2Emissions = isHP ? null : defaultUnit.CO2Emissions;
                        machine.FuelConsumption = isHP ? null : defaultUnit.FuelConsumption;
                        machine.ProductionCosts = defaultUnit.ProductionCosts;
                    }
                    viewModel.SaveMachine(machine);
                }
                
                // Update visual state for each textbox
                foreach (var textBox in textBoxes)
                {
                    bool isPermanentlyDisabled = false;
                    
                    if ((isGB || isOB) && textBox.Name == "ElectricityUsageTextBox")
                    {
                        isPermanentlyDisabled = true;
                        ConfigureFieldAsNA(card, "ElectricityUsageTextBox", "Electricity Usage (N/A)");
                    }
                    else if (isHP && (textBox.Name == "CO2EmissionsTextBox" || textBox.Name == "FuelConsumptionTextBox"))
                    {
                        isPermanentlyDisabled = true;
                        if (textBox.Name == "CO2EmissionsTextBox")
                            ConfigureFieldAsNA(card, "CO2EmissionsTextBox", "CO₂ Emissions (N/A)");
                        else
                            ConfigureFieldAsNA(card, "FuelConsumptionTextBox", "Fuel Consumption (N/A)");
                    }
                    else if (textBox.Name == "HeatOutputTextBox" ||
                           textBox.Name == "ElectricityUsageTextBox" ||
                           textBox.Name == "CO2EmissionsTextBox" ||
                           textBox.Name == "ProductionCostsTextBox" ||
                           textBox.Name == "FuelConsumptionTextBox")
                    {
                        if (!isPermanentlyDisabled)
                        {
                            textBox.IsEnabled = !isDisabled;
                            if (isDisabled)
                            {
                                textBox.Text = "0";
                            }
                            else
                            {
                                var value = textBox.Name switch
                                {
                                    "HeatOutputTextBox" => defaultUnit.MaxHeat,
                                    "ElectricityUsageTextBox" => defaultUnit.MaxElectricity,
                                    "CO2EmissionsTextBox" => defaultUnit.CO2Emissions,
                                    "ProductionCostsTextBox" => defaultUnit.ProductionCosts,
                                    "FuelConsumptionTextBox" => defaultUnit.FuelConsumption,
                                    _ => null
                                };
                                textBox.Text = value?.ToString(CultureInfo.InvariantCulture) ?? "0";
                            }
                        }
                    }
                }

                // Update visual disabled state
                card.Opacity = isDisabled ? 0.6 : 1.0;
                
                var machineImage = card.FindDescendantOfType<Image>();
                if (machineImage != null)
                {
                    machineImage.Opacity = isDisabled ? 0.5 : 1.0;
                }
                
                var disabledOverlay = card.FindDescendantOfType<TextBlock>(tb => tb.Name == "DisabledOverlayText");
                if (disabledOverlay != null)
                {
                    disabledOverlay.IsVisible = isDisabled;
                }
                
                var toggleButton = card.FindDescendantOfType<Button>(b => b.Name == "DisableEnableButton");
                if (toggleButton != null)
                {
                    var buttonText = toggleButton.FindDescendantOfType<TextBlock>(tb => tb.Name == "ToggleButtonText");
                    if (buttonText != null)
                    {
                        buttonText.Text = isDisabled ? "Enable Machine" : "Disable Machine";
                        toggleButton.Background = new SolidColorBrush(isDisabled ? 
                            Colors.DarkGreen : Color.Parse("#707070"));
                    }
                }
            }
        }
        
        // Clear all values when checkbox is unchecked
        private void ClearAllValues()
        {
            // Get all machine cards
            var itemsControl = this.FindControl<ItemsControl>("MachinesItemsControl");
            if (itemsControl == null) return;
            
            // Find all cards in the ItemsControl
            var cards = itemsControl.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 280 && b.Name != "ValidationBanner")
                .ToList();
            
            foreach (var card in cards)
            {
                // Find the machine name in this card
                var nameTextBlock = card.FindDescendantOfType<TextBlock>(tb => 
                    tb.FontSize == 16 && tb.FontWeight == FontWeight.Bold);
                
                if (nameTextBlock == null) continue;
                
                string machineName = nameTextBlock.Text ?? string.Empty;
                
                // Determine which fields should remain empty based on machine type
                bool isGB = machineName == "GB1" || machineName == "GB2";
                bool isOB = machineName == "OB1";
                bool isHP = machineName == "HP1";
                
                // Find all the text boxes for this machine
                var textBoxes = card.GetVisualDescendants().OfType<TextBox>().ToList();
                
                // Set values appropriately - 0 for regular fields, empty for permanently disabled fields
                foreach (var textBox in textBoxes)
                {
                    bool isPermanentlyDisabled = false;
                    
                    // Check if this is a permanently disabled field
                    if ((isGB || isOB) && textBox.Name == "ElectricityUsageTextBox") {
                        isPermanentlyDisabled = true;
                    }
                    else if (isHP && (textBox.Name == "CO2EmissionsTextBox" || textBox.Name == "FuelConsumptionTextBox")) {
                        isPermanentlyDisabled = true;
                    }
                    
                    // Set to empty for permanently disabled fields, 0 for others
                    if (isPermanentlyDisabled) {
                        textBox.Text = ""; // Keep permanently disabled fields empty
                    } else if (textBox.Name == "HeatOutputTextBox" || 
                        textBox.Name == "ElectricityUsageTextBox" || 
                        textBox.Name == "CO2EmissionsTextBox" || 
                        textBox.Name == "ProductionCostsTextBox" || 
                        textBox.Name == "FuelConsumptionTextBox") {
                        textBox.Text = "0";
                    }
                }
            }
            
            // Trigger save for each machine to persist changes
            if (DataContext is MachineryViewModel viewModel)
            {
                foreach (var machine in viewModel.Machines)
                {
                    bool isGB = machine.Name == "GB1" || machine.Name == "GB2";
                    bool isOB = machine.Name == "OB1";
                    bool isHP = machine.Name == "HP1";
                    
                    // Set the model values
                    machine.MaxHeat = 0;
                    
                    // Handle special cases for model properties
                    if (isGB || isOB) {
                        machine.MaxElectricity = null; // Keep null for GB and OB units
                    } else {
                        machine.MaxElectricity = 0;
                    }
                    
                    if (isHP) {
                        machine.CO2Emissions = null; // Keep null for HP unit
                        machine.FuelConsumption = null; // Keep null for HP unit
                    } else {
                        machine.CO2Emissions = 0;
                        machine.FuelConsumption = 0;
                    }
                    
                    machine.ProductionCosts = 0;
                    
                    viewModel.SaveMachine(machine);
                }
            }
            
            // Make sure special fields remain properly configured
            ConfigureSpecialFields();
            
            System.Diagnostics.Debug.WriteLine("Cleared all values (set to 0, kept special fields empty)");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            var infoButton = this.FindControl<Button>("InfoButton");
            var infoPopup = this.FindControl<Popup>("InfoPopup");
            
            if (infoButton != null && infoPopup != null)
            {
                infoButton.Click += (s, e) =>
                {
                    infoPopup.IsOpen = true;
                    e.Handled = true;
                };
            }
        }
        
        private void CloseInfoPopup_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindControl<Popup>("InfoPopup");
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }
    }
}
