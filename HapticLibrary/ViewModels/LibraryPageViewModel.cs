using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace HapticLibrary.ViewModels
{
    public class LibraryBookItem
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        public ICommand? ClickCommand { get; set; }
    }

    public class BookColorConverter : IValueConverter
    {
        public static readonly BookColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected 
                    ? new SolidColorBrush(Color.Parse("#007AFF")) // Blue for selected
                    : new SolidColorBrush(Color.Parse("#C7C7CC")); // More visible gray for unselected
            }
            return new SolidColorBrush(Color.Parse("#C7C7CC"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class F451VisibilityConverter : IValueConverter
    {
        public static readonly F451VisibilityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == "F451";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlaceholderVisibilityConverter : IValueConverter
    {
        public static readonly PlaceholderVisibilityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() != "F451";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LibraryPageViewModel : ViewModelBase, IPageViewModel
    {
        private readonly Action<string>? _navigateToReading;

        public ObservableCollection<LibraryBookItem> Books { get; }

        public LibraryPageViewModel(Action<string>? navigateToReading = null)
        {
            _navigateToReading = navigateToReading;
            
            Books = new ObservableCollection<LibraryBookItem>
            {
                new LibraryBookItem
                {
                    Title = "Fahrenheit 451",
                    Author = "Ray Bradbury",
                    CoverImagePath = "avares://HapticLibrary/Assets/Fahrenheit_451_Cover.jpg",
                    BookId = "F451",
                    IsSelected = true, // Set as selected for demo
                    ClickCommand = new RelayCommand(() => OpenBook("F451"))
                },
                new LibraryBookItem
                {
                    Title = "1984",
                    Author = "George Orwell",
                    CoverImagePath = "", // No HTTP URL - will show placeholder
                    BookId = "1984",
                    IsSelected = false,
                    ClickCommand = new RelayCommand(() => OpenBook("1984"))
                },
                new LibraryBookItem
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    CoverImagePath = "", // No HTTP URL - will show placeholder
                    BookId = "GATSBY",
                    IsSelected = false,
                    ClickCommand = new RelayCommand(() => OpenBook("GATSBY"))
                },
                new LibraryBookItem
                {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    CoverImagePath = "", // No HTTP URL - will show placeholder
                    BookId = "MOCKINGBIRD",
                    IsSelected = false,
                    ClickCommand = new RelayCommand(() => OpenBook("MOCKINGBIRD"))
                }
            };
        }

        private void OpenBook(string bookId)
        {
            if (bookId == "F451")
            {
                // Navigate to reading page for F451
                _navigateToReading?.Invoke(bookId);
            }
            else
            {
                // For other books, just show a message or do nothing for now
                // Could show a "Coming Soon" dialog or similar
            }
        }
    }
}
