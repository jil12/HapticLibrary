using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

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
    public void ShowLibraryPage()
    {
        DisposeCurrentPage();
        CurrentPage = new LibraryPageViewModel(NavigateToReadingWithBook);
    }
    
    public void ShowReadingPage()
    {
        DisposeCurrentPage();
        CurrentPage = new ReadingPageViewModel();
    }

    public void ShowEditorPage()
    {
        DisposeCurrentPage();
        CurrentPage = new HapticEditorViewModel();
    }

    /// <summary>
    /// Navigate to reading page with a specific book and reading mode
    /// </summary>
    /// <param name="bookId">The ID of the book to open</param>
    /// <param name="readingMode">The reading mode ("audio" or "read-aloud")</param>
    private void NavigateToReadingWithBook(string bookId, string readingMode)
    {
        // Load the specific book and navigate to reading page with the specified mode
        DisposeCurrentPage();
        CurrentPage = new ReadingPageViewModel(bookId, readingMode);
        // Update navigation state
        NavigationBar.IsLibrarySelected = false;
        NavigationBar.IsReadingSelected = true;
        NavigationBar.IsEditorSelected = false;
    }
    
    /// <summary>
    /// Dispose of the current page if it implements IDisposable
    /// </summary>
    private void DisposeCurrentPage()
    {
        if (CurrentPage is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
