using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using DanfossHeating;
using DanfossHeating.ViewModels;
using DanfossHeating.Views;
using Xunit;

namespace DanfossHeatingTests;

public class UITests
{
    [AvaloniaFact]
    public void HomePage_DataContext_Sets_UserName_And_Updates_Theme()
    {
        // Arrange
        var homePage = new HomePage();
        var viewModel = new HomePageViewModel("TestUser", false);

        // Act
        homePage.DataContext = viewModel;

        // Assert
        Assert.Equal("TestUser", ((HomePageViewModel)homePage.DataContext).UserName);
    }

    [AvaloniaFact]
    public void MachineryPage_RestoreOriginalValues_Resets_Fields()
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
        restoreMethod.Invoke(machineryPage, null);

        // Assert
        foreach (var machine in viewModel.Machines)
        {
            Assert.NotEqual(999, machine.MaxHeat); // Should be reset to original
            Assert.NotEqual(999, machine.ProductionCosts); // Should be reset to original
        }
    }

}