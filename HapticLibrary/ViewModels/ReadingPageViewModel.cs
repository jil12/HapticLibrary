using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HapticLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class ReadingPageViewModel : ViewModelBase, IPageViewModel
    {
        /// <summary>
        /// Displays current page on main view
        /// </summary>
        [ObservableProperty]
        private string? _bookLine = "BookNotLoaded";
        [ObservableProperty]
        private int? _pageNumber = 0;


        private ReadingBook _readingBook = new ReadingBook();

        public ReadingPageViewModel()
        {
            _readingBook.LoadBook("test");
            _bookLine = _readingBook.GetText();
        }

        [RelayCommand]
        public void NextPage()
        {
            _readingBook.NextPage();
            BookLine = _readingBook.GetText();
            PageNumber = _readingBook.PageIndex + 1;
        }

        [RelayCommand]
        public void PreviousPage()
        {
            _readingBook.PreviousPage();
            BookLine = _readingBook.GetText();
            PageNumber = _readingBook.PageIndex + 1;
        }
    }
}
