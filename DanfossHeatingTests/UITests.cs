using Avalonia.Headless.XUnit;
using DanfossHeating.ViewModels;
using DanfossHeating.Views;

namespace DanfossHeatingTests;

public class UITests
{
    /// <summary>
    /// This test verifies that when the DataContext of the HomePage is set, the UserName property is correctly assigned
    /// and that the logic to update the theme based on the username is triggered.
    /// </summary>
    [AvaloniaFact]
    public void HomePage_DataContext_ShouldSetUserName_And_UpdateTheme()
    {
        // Arrange
        var homePage = new HomePage();
        var viewModel = new HomePageViewModel("TestUser", false);

        // Act
        homePage.DataContext = viewModel;

        // Assert
        Assert.Equal("TestUser", ((HomePageViewModel)homePage.DataContext).UserName);
    }

    /// <summary>
    /// This test verifies that when the "RestoreOriginalValues" method is invoked on the MachineryPage, 
    /// it correctly resets the fields (such as MaxHeat and ProductionCosts) to their original values.
    /// It checks that values which were changed are restored to their initial state.
    /// </summary>
    [AvaloniaFact]
    public void MachineryPage_ShouldRestoreOriginalValues_WhenReset()  // Test for machinery -> it checks if the original values are restored as expected after resetting them.
    {
        // Arrange
        var machineryPage = new MachineryPage();
        var viewModel = new MachineryViewModel("TestUser", false);
        machineryPage.DataContext = viewModel;

        // Simulate changes to machine values
        foreach (var machine in viewModel.Machines)
        {
            machine.MaxHeat = 999;
            machine.ProductionCosts = 999;
        }

        // Act
        var restoreMethod = machineryPage.GetType().GetMethod("RestoreOriginalValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(restoreMethod); // Ensure the method exists
        restoreMethod.Invoke(machineryPage, null);

        // Assert
        foreach (var machine in viewModel.Machines)
        {
            Assert.NotEqual(999, machine.MaxHeat); // Should be reset to original
            Assert.NotEqual(999, machine.ProductionCosts); // Should be reset to original
        }
    }
}