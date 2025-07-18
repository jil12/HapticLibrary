using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
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
            _main.ShowLibraryPage();
        }

        /// <summary>
        /// Command to change content of page to the reading page.
        /// <see cref="ViewModels.MainViewModel"/>
        /// </summary>
        [RelayCommand]
        private void NavigateToReadingPage()
        {
            _main.ShowReadingPage();
        }

        /// <summary>
        /// Command to change content of page to the settings.
        /// </summary>
        [RelayCommand]
        private void NavigateToSettingsPage()
        {
            _main.ShowSettingsPage();
        }

        [RelayCommand]
        private void NavigateToEditorPage()
        {
            _main.ShowEditorPage();
        }
    }
}
