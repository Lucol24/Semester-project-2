using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DanfossHeating;
using Xunit;

namespace DanfossHeatingTests;

public class SourceDataManagerTests
{
    private readonly string _filePath;
    private readonly bool _fileExists;

    public SourceDataManagerTests()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\..\..\");
        _filePath = Path.GetFullPath("DanfossHeating/Data/heat_demand.csv");
        _fileExists = File.Exists(_filePath);
    }

    [Fact]
    public void File_Exists_WhenCSVFileIsPresent()
    {
        Console.WriteLine("\n==> [SDM] Testing CSV file existence");
        Assert.True(_fileExists, "Heat Demand CSV file is missing!");
        Console.WriteLine("|> Test complete - CSV file exists and is accessible\n");
    }

    [SkippableFact]
    public void LoadHeatDemand_EnsuresWinterAndSummerDataExist()
    {
        Skip.IfNot(_fileExists, "Skipping test: Heat Demand CSV file is missing!");

        Console.WriteLine("\n==> [SDM] Testing seasonal heat demand data");
        var sourceDataManager = new SourceDataManager();
        var winterDemands = sourceDataManager.GetWinterHeatDemands();
        var summerDemands = sourceDataManager.GetSummerHeatDemands();
       
        Assert.NotNull(winterDemands);
        Assert.NotEmpty(winterDemands);
        Assert.NotNull(summerDemands);
        Assert.NotEmpty(summerDemands);

        Console.WriteLine("|> Test complete - Found valid seasonal data:");
        Console.WriteLine($"    Winter entries: {winterDemands.Count}");
        Console.WriteLine($"    Summer entries: {summerDemands.Count}\n");
    }

    [SkippableFact]
    public void AllHeatDemands_ShouldHaveElectricityPriceGreaterThanZero()
    {
        Skip.IfNot(_fileExists, "Skipping test: Heat Demand CSV file is missing!");

        Console.WriteLine("\n==> [SDM] Testing electricity price validation");
        var sourceDataManager = new SourceDataManager();
        List<HeatDemand> winterHeatDemands = sourceDataManager.GetWinterHeatDemands();
        List<HeatDemand> summerHeatDemands = sourceDataManager.GetSummerHeatDemands();

        foreach (var demand in winterHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, 
                $"Winter demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }
        
        foreach (var demand in summerHeatDemands)
        {
            Assert.True(demand.ElectricityPrice > 0, 
                $"Summer demand at {demand.TimeFrom} has invalid ElectricityPrice: {demand.ElectricityPrice}");
        }

        Console.WriteLine("|> Test complete - All electricity prices are valid:");
        Console.WriteLine($"    Winter entries checked: {winterHeatDemands.Count}");
        Console.WriteLine($"    Summer entries checked: {summerHeatDemands.Count}\n");
    }
}