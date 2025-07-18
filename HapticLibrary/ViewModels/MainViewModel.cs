using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;

namespace HapticLibrary.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// Displays current page on main view
    /// </summary>
    [ObservableProperty]
    private IPageViewModel? _currentPage;



    /// <summary>
    /// Navigation bar that's displayed at the bottom of the screen
    /// </summary>
    public NavigationBarViewModel NavigationBar { get; }

    /// <summary>
    /// Default Constructor: opens to the library page by default.
    /// </summary>
    public MainViewModel()
    {
        NavigationBar = new NavigationBarViewModel(this);
        ShowLibraryPage();
        HapticManager hapticManager = HapticManager.GetInstance();
        hapticManager.StartManager();   //Make sure manager is running
    }

    /// <summary>
    /// Methods called by <see cref="ViewModels.NavigationBarViewModel"/> commands
    /// to display different pages
    /// </summary>
    public void ShowLibraryPage() => CurrentPage = new LibraryPageViewModel();
    public void ShowReadingPage() => CurrentPage = new ReadingPageViewModel();
    public void ShowSettingsPage() => CurrentPage = new SettingsPageViewModel();
    public void ShowEditorPage() => CurrentPage = new HapticEditorViewModel();
}
