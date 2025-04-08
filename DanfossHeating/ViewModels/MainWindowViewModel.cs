using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Platform;
using System.Threading.Tasks;

namespace DanfossHeating.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _isDarkTheme = false;
    private Bitmap? _darkBackgroundImage;
    private Bitmap? _lightBackgroundImage;
    private Bitmap? _currentBackgroundImage;
    private string _userName = string.Empty;
    private bool _isLoading = false;
    private double _loadingProgress = 0;
    private object? _currentPage;
    
    public event EventHandler<NavigationEventArgs>? NavigateToPage;

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                UpdateTheme(value);
            }
        }
    }
    
    public Bitmap? CurrentBackgroundImage
    {
        get => _currentBackgroundImage;
        private set => SetProperty(ref _currentBackgroundImage, value);
    }
    
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }
    
    public double LoadingProgress
    {
        get => _loadingProgress;
        private set => SetProperty(ref _loadingProgress, value);
    }
    
    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }
    
    public ICommand ToggleThemeCommand { get; }
    public ICommand EnterCommand { get; }
    
    public override PageType PageType => PageType.Login;
    
    public MainWindowViewModel()
    {
        ToggleThemeCommand = new Command(ToggleTheme);
        EnterCommand = new Command(ProcessNameWithAnimation);
        
        try
        {
            _lightBackgroundImage = LoadAssetImage("Light_background.png");
            _darkBackgroundImage = LoadAssetImage("Dark_background.png");
            
            CurrentBackgroundImage = _lightBackgroundImage;
            
            Console.WriteLine("Images loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading background images: {ex.Message}");
        }
    }
    
    private async void ProcessNameWithAnimation()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            Console.WriteLine("No username entered, skipping login");
            return;
        }
            
        IsLoading = true;
        LoadingProgress = 0;
        
        try
        {
            Console.WriteLine("Starting loading animation...");
            // Animate progress from 0 to 100%
            for (int progress = 0; progress <= 100; progress += 2)
            {
                LoadingProgress = progress;
                await Task.Delay(40); // Speed for Loading ...
                
                // Checkpoint in the middle of the animation
                if (progress == 50)
                    Console.WriteLine("Loading animation 50% complete");
            }
            
            Console.WriteLine($"Name entered: {UserName}, loading complete");
            
            // Wait a tiny bit before navigating
            await Task.Delay(100);
            NavigateToHomePage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during processing: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
            LoadingProgress = 0;
        }
    }

    private void NavigateToHomePage()
    {
        Console.WriteLine("Creating HomePageViewModel and raising NavigateToPage event");
        var homeViewModel = new HomePageViewModel(UserName, IsDarkTheme);
        homeViewModel.SetMainViewModel(this);
        NavigateTo(homeViewModel);
    }
    
    public void NavigateTo(ViewModelBase viewModel)
    {
        Console.WriteLine($"Navigating to {viewModel.PageType}");
        
        if (NavigateToPage == null)
        {
            Console.WriteLine("NavigateToPage event is null");
            return;
        }
        
        NavigateToPage.Invoke(this, new NavigationEventArgs(viewModel));
        Console.WriteLine("NavigateToPage event raised");
    }
    
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }
    
    private void UpdateTheme(bool isDark)
    {
        // Update theme
        Application.Current!.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
        
        // Update the background image
        try
        {
            CurrentBackgroundImage = isDark ? _darkBackgroundImage : _lightBackgroundImage;
            Console.WriteLine($"Theme changed to {(isDark ? "Dark" : "Light")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting background image: {ex.Message}");
        }
    }
    
    private static Bitmap? LoadAssetImage(string fileName)
    {
        try
        {
            var assets = AssetLoader.Open(new Uri($"avares://DanfossHeating/Assets/{fileName}"));
            return new Bitmap(assets);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load image {fileName}: {ex.Message}");
            return null;
        }
    }
}

public class NavigationEventArgs : EventArgs
{
    public object ViewModel { get; }

    public NavigationEventArgs(object viewModel)
    {
        ViewModel = viewModel;
    }
}

public class Command : ICommand
{
    private readonly Action _execute;
    
    public Command(Action execute)
    {
        _execute = execute;
    }
    
    public bool CanExecute(object? parameter) => true;
    
    public void Execute(object? parameter) => _execute();
    
    // This event is required by the ICommand interface but we don't use it
    // Suppressing the warning with pragma
    #pragma warning disable 0067
    public event EventHandler? CanExecuteChanged;
    #pragma warning restore 0067
}