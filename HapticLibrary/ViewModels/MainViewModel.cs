using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    }

    /// <summary>
    /// Methods called by <see cref="ViewModels.NavigationBarViewModel"/> commands
    /// to display different pages
    /// </summary>
    public void ShowLibraryPage() => CurrentPage = new LibraryPageViewModel(NavigateToReadingWithBook);
    public void ShowReadingPage() => CurrentPage = new ReadingPageViewModel();

    public void ShowSettingsPage() => CurrentPage = new SettingsPageViewModel();

    /// <summary>
    /// Navigate to reading page with a specific book
    /// </summary>
    /// <param name="bookId">The ID of the book to open</param>
    private void NavigateToReadingWithBook(string bookId)
    {
        // Load the specific book and navigate to reading page
        ShowReadingPage();
        // Update navigation state
        NavigationBar.IsLibrarySelected = false;
        NavigationBar.IsReadingSelected = true;
        NavigationBar.IsSettingsSelected = false;
        // You can pass the bookId to ReadingPageViewModel if needed
    }
}
