using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia;

namespace HapticLibrary.ViewModels
{
    public class NavigationIconConverter : IValueConverter
    {
        public static readonly NavigationIconConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected
                    ? new SolidColorBrush(Color.Parse("#85CCB7")) // Gradient middle color for selected
                    : new SolidColorBrush(Color.Parse("#f8f9fa")); // Gray for unselected
            }
            return new SolidColorBrush(Color.Parse("#f8f9fa"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NavigationTextConverter : IValueConverter
    {
        public static readonly NavigationTextConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected
                    ? new SolidColorBrush(Color.Parse("#85CCB7")) // Gradient middle color for selected
                    : new SolidColorBrush(Color.Parse("#8E8E93")); // Gray for unselected
            }
            return new SolidColorBrush(Color.Parse("#8E8E93"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class to represent the Navigation Bar at the bottom of the screen.
    /// </summary>
    public partial class NavigationBarViewModel : ViewModelBase
    {
        /// <summary>
        /// Instance variable that contains a reference to the MainViewModel.
        /// This is used to change the main page by clicking on the navigation bar.
        /// </summary>
        private readonly MainViewModel _main;

        [ObservableProperty]
        private bool _isLibrarySelected = true;

        [ObservableProperty]
        private bool _isReadingSelected = false;

        [ObservableProperty]
        private bool _isEditorSelected = false;

        /// <summary>
        /// Creates an instance of a NavigationBar
        /// </summary>
        /// <param name="main">MainViewModel</param>
        public NavigationBarViewModel(MainViewModel main)
        {
            _main = main;
        }

        /// <summary>
        /// Command to change content of page to the library.
        /// <see cref="ViewModels.MainViewModel"/>
        /// </summary>
        [RelayCommand]
        private void NavigateToLibraryPage()
        {
            IsLibrarySelected = true;
            IsReadingSelected = false;
            IsEditorSelected = false;
            _main.ShowLibraryPage();
        }

        /// <summary>
        /// Command to change content of page to the reading page.
        /// <see cref="ViewModels.MainViewModel"/>
        /// </summary>
        [RelayCommand]
        private void NavigateToReadingPage()
        {
            IsLibrarySelected = false;
            IsReadingSelected = true;
            IsEditorSelected = false;
            _main.ShowReadingPage();
        }

        /// <summary>
        /// Command to change content of page to the Editor.
        /// </summary>
        [RelayCommand]
        private void NavigateToEditorPage()
        {
            IsLibrarySelected = false;
            IsReadingSelected = false;
            IsEditorSelected = true;
            _main.ShowEditorPage();
        }
    }
}
