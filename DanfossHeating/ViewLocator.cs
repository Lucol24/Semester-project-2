using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DanfossHeating.ViewModels;

namespace DanfossHeating;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "No data" };

        // Don't attempt to load PageTemplate view
        var name = data.GetType().FullName!.Replace("ViewModel", "");
        if (name.Contains("PageTemplate"))
            return new TextBlock { Text = "PageTemplate view is not supported" };

        var type = Type.GetType(name);

        if (type != null)
        {
            try
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"ViewLocator error creating {type.FullName}: {ex.Message}");
                return new TextBlock { Text = $"Error loading view: {ex.Message}" };
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
