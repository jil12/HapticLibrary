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
        public string ReadingMode { get; set; } = "audio"; // "audio" or "read-aloud"
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

    public class LoraxVisibilityConverter : IValueConverter
    {
        public static readonly LoraxVisibilityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == "LORAX";
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
            return value?.ToString() != "F451" && value?.ToString() != "LORAX";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LibraryPageViewModel : ViewModelBase, IPageViewModel
    {
        private readonly Action<string, string>? _navigateToReading; // bookId, readingMode

        public ObservableCollection<LibraryBookItem> AudioOnlyBooks { get; }
        public ObservableCollection<LibraryBookItem> ReadAloudBooks { get; }

        public LibraryPageViewModel(Action<string, string>? navigateToReading = null)
        {
            _navigateToReading = navigateToReading;
            
            AudioOnlyBooks = new ObservableCollection<LibraryBookItem>
            {
                new LibraryBookItem
                {
                    Title = "Fahrenheit 451",
                    Author = "Ray Bradbury",
                    CoverImagePath = "avares://HapticLibrary/Assets/Fahrenheit_451_Cover.jpg",
                    BookId = "F451",
                    ReadingMode = "audio",
                    IsSelected = false,
                    ClickCommand = new RelayCommand(() => OpenBook("F451", "audio"))
                }
            };

            ReadAloudBooks = new ObservableCollection<LibraryBookItem>
            {
                new LibraryBookItem
                {
                    Title = "The Lorax",
                    Author = "Dr. Seuss",
                    CoverImagePath = "avares://HapticLibrary/Assets/TheLoraxCover.jpg",
                    BookId = "LORAX",
                    ReadingMode = "read-aloud",
                    IsSelected = false,
                    ClickCommand = new RelayCommand(() => OpenBook("LORAX", "read-aloud"))
                }
            };
        }

        private void OpenBook(string bookId, string readingMode)
        {
            // Navigate to reading page with the specified book and mode
            _navigateToReading?.Invoke(bookId, readingMode);
        }
    }
}
